namespace RunLoki365.Models;

/// <summary>
/// Storage information structure.
/// </summary>
public readonly struct StorageInfo
{
    /// <summary>
    /// Total storage in bytes.
    /// </summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Used storage in bytes.
    /// </summary>
    public long UsedBytes { get; init; }

    /// <summary>
    /// Available storage in bytes.
    /// </summary>
    public long AvailableBytes { get; init; }

    /// <summary>
    /// Storage usage percentage (0-100).
    /// </summary>
    public double UsagePercent { get; init; }

    /// <summary>
    /// Formats used storage as human-readable string.
    /// </summary>
    public string FormatUsed() => FormatBytes(UsedBytes);

    /// <summary>
    /// Formats total storage as human-readable string.
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
