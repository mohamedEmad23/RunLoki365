namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for CPU monitoring functionality.
/// </summary>
public interface ICpuMonitor
{
    /// <summary>
    /// Gets the current smoothed CPU usage percentage (0-100).
    /// </summary>
    double GetCurrentUsage();

    /// <summary>
    /// Starts CPU monitoring with periodic sampling.
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// Stops CPU monitoring.
    /// </summary>
    void StopMonitoring();
}
