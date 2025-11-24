using RunLoki365.Interfaces;
using RunLoki365.Models;
using Serilog;

namespace RunLoki365.Monitors;

/// <summary>
/// Memory monitor implementation using /proc/meminfo parsing.
/// </summary>
public class ProcMemInfoMonitor : IMemoryMonitor
{
    private const string ProcMemInfoPath = "/proc/meminfo";
    private readonly ILogger _logger;

    public ProcMemInfoMonitor(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public MemoryInfo GetMemoryInfo()
    {
        try
        {
            var lines = File.ReadAllLines(ProcMemInfoPath);
            
            var memTotal = ParseMemInfoField(lines, "MemTotal:");
            var memAvailable = ParseMemInfoField(lines, "MemAvailable:");

            if (memTotal == 0)
            {
                _logger.Warning("MemTotal is 0, returning empty MemoryInfo");
                return new MemoryInfo();
            }

            var memUsed = memTotal - memAvailable;
            var usagePercent = 100.0 * memUsed / memTotal;

            return new MemoryInfo
            {
                TotalBytes = memTotal,
                AvailableBytes = memAvailable,
                UsedBytes = memUsed,
                UsagePercent = usagePercent
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read memory information from /proc/meminfo");
            return new MemoryInfo();
        }
    }

    /// <summary>
    /// Parses a field from /proc/meminfo lines.
    /// </summary>
    /// <param name="lines">Lines from /proc/meminfo.</param>
    /// <param name="fieldName">Field name to search for (e.g., "MemTotal:").</param>
    /// <returns>Value in bytes.</returns>
    private long ParseMemInfoField(string[] lines, string fieldName)
    {
        var line = lines.FirstOrDefault(l => l.StartsWith(fieldName));
        if (line == null)
        {
            _logger.Warning("Field {FieldName} not found in /proc/meminfo", fieldName);
            return 0;
        }

        // Format: "MemTotal:       16384000 kB"
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            _logger.Warning("Invalid format for {FieldName}: {Line}", fieldName, line);
            return 0;
        }

        if (!long.TryParse(parts[1], out var valueKb))
        {
            _logger.Warning("Failed to parse value for {FieldName}: {Value}", fieldName, parts[1]);
            return 0;
        }

        // Convert from kB to bytes
        return valueKb * 1024;
    }
}
