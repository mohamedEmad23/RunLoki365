using RunLoki365.Models;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for system monitoring functionality.
/// Aggregates CPU, memory, and storage metrics.
/// </summary>
public interface ISystemMonitor
{
    /// <summary>
    /// Event fired when CPU usage changes.
    /// </summary>
    event EventHandler<double>? CpuUsageChanged;

    /// <summary>
    /// Gets the current CPU usage percentage (0-100).
    /// </summary>
    double GetCpuUsage();

    /// <summary>
    /// Gets current memory information.
    /// </summary>
    MemoryInfo GetMemoryInfo();

    /// <summary>
    /// Gets current storage information for the home directory partition.
    /// </summary>
    StorageInfo GetStorageInfo();

    /// <summary>
    /// Starts monitoring system metrics.
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// Stops monitoring system metrics.
    /// </summary>
    void StopMonitoring();
}
