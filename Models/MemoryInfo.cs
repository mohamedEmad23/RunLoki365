namespace RunLoki365.Models;

/// <summary>
/// Memory information structure.
/// </summary>
public readonly struct MemoryInfo
{
    /// <summary>
    /// Total memory in bytes.
    /// </summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Available memory in bytes.
    /// </summary>
    public long AvailableBytes { get; init; }

    /// <summary>
    /// Used memory in bytes.
    /// </summary>
    public long UsedBytes { get; init; }

    /// <summary>
    /// Memory usage percentage (0-100).
    /// </summary>
    public double UsagePercent { get; init; }

    /// <summary>
    /// Formats used memory as human-readable string.
    /// </summary>
    public string FormatUsed() => FormatBytes(UsedBytes);

    /// <summary>
    /// Formats total memory as human-readable string.
    /// </summary>
    public string FormatTotal() => FormatBytes(TotalBytes);

    /// <summary>
    /// Formats bytes to GiB with 1 decimal place.
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        return $"{bytes / 1024.0 / 1024.0 / 1024.0:F1} GiB";
    }
}
