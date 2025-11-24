using RunLoki365.Models;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for storage monitoring functionality.
/// </summary>
public interface IStorageMonitor
{
    /// <summary>
    /// Gets storage information for the specified path.
    /// </summary>
    /// <param name="path">Path to check storage for (typically home directory).</param>
    StorageInfo GetStorageInfo(string path);
}
