using RunLoki365.Models;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for memory monitoring functionality.
/// </summary>
public interface IMemoryMonitor
{
    /// <summary>
    /// Gets current memory information.
    /// </summary>
    MemoryInfo GetMemoryInfo();
}
