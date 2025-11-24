using RunLoki365.Interfaces;
using Serilog;

namespace RunLoki365.Monitors;

/// <summary>
/// CPU monitor implementation using /proc/stat parsing.
/// </summary>
public class ProcStatCpuMonitor : ICpuMonitor, IDisposable
{
    private const string ProcStatPath = "/proc/stat";
    private const double SmoothingAlpha = 0.3;
    private const int SamplingIntervalMs = 1000;

    private readonly ILogger _logger;
    private Timer? _samplingTimer;
    private double _smoothedUsage;
    private (long idle, long total) _previousSample;
    private bool _isMonitoring;
    private readonly object _lock = new();

    public ProcStatCpuMonitor(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public double GetCurrentUsage()
    {
        lock (_lock)
        {
            return _smoothedUsage;
        }
    }

    /// <inheritdoc/>
    public void StartMonitoring()
    {
        lock (_lock)
        {
            if (_isMonitoring)
            {
                return;
            }

            _logger.Information("Starting CPU monitoring");
            _isMonitoring = true;

            // Take initial sample
            try
            {
                _previousSample = ReadProcStat();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to read initial /proc/stat sample");
                _previousSample = (0, 0);
            }

            // Start sampling timer
            _samplingTimer = new Timer(
                SampleCallback,
                null,
                SamplingIntervalMs,
                SamplingIntervalMs
            );
        }
    }

    /// <inheritdoc/>
    public void StopMonitoring()
    {
        lock (_lock)
        {
            if (!_isMonitoring)
            {
                return;
            }

            _logger.Information("Stopping CPU monitoring");
            _isMonitoring = false;

            _samplingTimer?.Dispose();
            _samplingTimer = null;
        }
    }

    private void SampleCallback(object? state)
    {
        try
        {
            var currentSample = ReadProcStat();

            lock (_lock)
            {
                var idleDelta = currentSample.idle - _previousSample.idle;
                var totalDelta = currentSample.total - _previousSample.total;

                if (totalDelta > 0)
                {
                    // Calculate raw CPU usage: 100 * (1 - idle_delta / total_delta)
                    var rawUsage = 100.0 * (1.0 - (double)idleDelta / totalDelta);

                    // Apply exponential smoothing
                    _smoothedUsage = (SmoothingAlpha * rawUsage) + ((1 - SmoothingAlpha) * _smoothedUsage);

                    _logger.Debug("CPU usage: {RawUsage:F1}% (raw), {SmoothedUsage:F1}% (smoothed)", 
                        rawUsage, _smoothedUsage);
                }

                _previousSample = currentSample;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to sample CPU usage from /proc/stat");
        }
    }

    /// <summary>
    /// Reads and parses /proc/stat to extract CPU time fields.
    /// </summary>
    /// <returns>Tuple of (idle time, total time) in jiffies.</returns>
    private (long idle, long total) ReadProcStat()
    {
        var lines = File.ReadAllLines(ProcStatPath);
        
        // First line contains aggregate CPU stats: "cpu user nice system idle iowait irq softirq..."
        var cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
        if (cpuLine == null)
        {
            throw new InvalidOperationException("Could not find 'cpu' line in /proc/stat");
        }

        // Parse CPU time fields
        var fields = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length < 5)
        {
            throw new InvalidOperationException($"Invalid /proc/stat format: {cpuLine}");
        }

        // Fields: cpu user nice system idle iowait irq softirq steal guest guest_nice
        // We need at least: user(1) nice(2) system(3) idle(4)
        var user = long.Parse(fields[1]);
        var nice = long.Parse(fields[2]);
        var system = long.Parse(fields[3]);
        var idle = long.Parse(fields[4]);
        
        // Optional fields (may not be present on all systems)
        var iowait = fields.Length > 5 ? long.Parse(fields[5]) : 0;
        var irq = fields.Length > 6 ? long.Parse(fields[6]) : 0;
        var softirq = fields.Length > 7 ? long.Parse(fields[7]) : 0;
        var steal = fields.Length > 8 ? long.Parse(fields[8]) : 0;

        // Total = sum of all time fields
        var total = user + nice + system + idle + iowait + irq + softirq + steal;

        return (idle, total);
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}
