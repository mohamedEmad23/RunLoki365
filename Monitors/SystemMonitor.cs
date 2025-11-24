using RunLoki365.Interfaces;
using RunLoki365.Models;
using Serilog;

namespace RunLoki365.Monitors;

/// <summary>
/// System monitor aggregator that coordinates CPU, memory, and storage monitoring.
/// Implements the ISystemMonitor interface by delegating to specialized monitor implementations.
/// </summary>
public class SystemMonitor : ISystemMonitor, IDisposable
{
    private readonly ICpuMonitor _cpuMonitor;
    private readonly IMemoryMonitor _memoryMonitor;
    private readonly IStorageMonitor _storageMonitor;
    private readonly ILogger _logger;
    private readonly string _homeDirectory;
    private Timer? _cpuCheckTimer;
    private double _lastCpuUsage;
    private bool _isMonitoring;
    private readonly object _lock = new();

    /// <inheritdoc/>
    public event EventHandler<double>? CpuUsageChanged;

    /// <summary>
    /// Initializes a new instance of the SystemMonitor class.
    /// </summary>
    /// <param name="cpuMonitor">CPU monitor implementation.</param>
    /// <param name="memoryMonitor">Memory monitor implementation.</param>
    /// <param name="storageMonitor">Storage monitor implementation.</param>
    /// <param name="logger">Logger instance.</param>
    public SystemMonitor(
        ICpuMonitor cpuMonitor,
        IMemoryMonitor memoryMonitor,
        IStorageMonitor storageMonitor,
        ILogger logger)
    {
        _cpuMonitor = cpuMonitor ?? throw new ArgumentNullException(nameof(cpuMonitor));
        _memoryMonitor = memoryMonitor ?? throw new ArgumentNullException(nameof(memoryMonitor));
        _storageMonitor = storageMonitor ?? throw new ArgumentNullException(nameof(storageMonitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Get home directory for storage monitoring
        _homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(_homeDirectory))
        {
            _homeDirectory = Environment.GetEnvironmentVariable("HOME") ?? "/home";
            _logger.Warning("Could not determine user home directory, using {HomeDirectory}", _homeDirectory);
        }
    }

    /// <inheritdoc/>
    public double GetCpuUsage()
    {
        try
        {
            return _cpuMonitor.GetCurrentUsage();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get CPU usage");
            return 0.0;
        }
    }

    /// <inheritdoc/>
    public MemoryInfo GetMemoryInfo()
    {
        try
        {
            return _memoryMonitor.GetMemoryInfo();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get memory information");
            return new MemoryInfo();
        }
    }

    /// <inheritdoc/>
    public StorageInfo GetStorageInfo()
    {
        try
        {
            return _storageMonitor.GetStorageInfo(_homeDirectory);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get storage information for {Path}", _homeDirectory);
            return new StorageInfo();
        }
    }

    /// <inheritdoc/>
    public void StartMonitoring()
    {
        lock (_lock)
        {
            if (_isMonitoring)
            {
                _logger.Warning("SystemMonitor is already monitoring");
                return;
            }

            _logger.Information("Starting system monitoring");
            _isMonitoring = true;

            try
            {
                // Start CPU monitoring (which has its own internal timer)
                _cpuMonitor.StartMonitoring();

                // Start a timer to check for CPU usage changes and fire events
                // Check every 1 second to detect changes and fire CpuUsageChanged event
                _cpuCheckTimer = new Timer(
                    CheckCpuUsageCallback,
                    null,
                    1000, // Initial delay
                    1000  // Period
                );

                _logger.Information("System monitoring started successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start system monitoring");
                _isMonitoring = false;
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public void StopMonitoring()
    {
        lock (_lock)
        {
            if (!_isMonitoring)
            {
                _logger.Warning("SystemMonitor is not currently monitoring");
                return;
            }

            _logger.Information("Stopping system monitoring");
            _isMonitoring = false;

            try
            {
                // Stop CPU monitoring
                _cpuMonitor.StopMonitoring();

                // Stop the CPU check timer
                _cpuCheckTimer?.Dispose();
                _cpuCheckTimer = null;

                _logger.Information("System monitoring stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while stopping system monitoring");
            }
        }
    }

    /// <summary>
    /// Timer callback to check for CPU usage changes and fire the CpuUsageChanged event.
    /// </summary>
    private void CheckCpuUsageCallback(object? state)
    {
        try
        {
            var currentUsage = GetCpuUsage();

            // Fire event if CPU usage has changed
            // Use a small threshold to avoid firing events for tiny fluctuations
            if (Math.Abs(currentUsage - _lastCpuUsage) > 0.01)
            {
                _lastCpuUsage = currentUsage;
                
                // Fire event on a separate thread to avoid blocking the timer
                Task.Run(() =>
                {
                    try
                    {
                        CpuUsageChanged?.Invoke(this, currentUsage);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error in CpuUsageChanged event handler");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in CPU usage check callback");
        }
    }

    /// <summary>
    /// Disposes resources used by the SystemMonitor.
    /// </summary>
    public void Dispose()
    {
        StopMonitoring();

        // Dispose CPU monitor if it implements IDisposable
        if (_cpuMonitor is IDisposable disposableCpuMonitor)
        {
            disposableCpuMonitor.Dispose();
        }
    }
}
