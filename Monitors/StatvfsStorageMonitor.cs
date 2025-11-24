using System.Runtime.InteropServices;
using RunLoki365.Interfaces;
using RunLoki365.Models;
using Serilog;

namespace RunLoki365.Monitors;

/// <summary>
/// Storage monitor implementation using statvfs() system call.
/// </summary>
public class StatvfsStorageMonitor : IStorageMonitor
{
    private readonly ILogger _logger;

    public StatvfsStorageMonitor(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public StorageInfo GetStorageInfo(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                _logger.Error("Path is null or empty");
                return new StorageInfo();
            }

            var stat = new StatVfs
            {
                __f_spare = new int[5]  // Initialize the spare array
            };
            var result = statvfs(path, ref stat);

            if (result != 0)
            {
                var errno = Marshal.GetLastWin32Error();
                _logger.Error("statvfs() failed for path {Path} with errno {Errno}", path, errno);
                return new StorageInfo
                {
                    TotalBytes = 0,
                    UsedBytes = 0,
                    AvailableBytes = 0,
                    UsagePercent = 0.0
                };
            }

            // Calculate storage values
            // f_blocks: total data blocks in filesystem
            // f_bfree: free blocks in filesystem
            // f_bavail: free blocks available to unprivileged user
            // f_frsize: fragment size (or f_bsize if f_frsize is 0)
            var blockSize = stat.f_frsize != 0 ? stat.f_frsize : stat.f_bsize;
            var totalBytes = (long)(stat.f_blocks * blockSize);
            var availableBytes = (long)(stat.f_bavail * blockSize);
            var usedBytes = totalBytes - (long)(stat.f_bfree * blockSize);
            var usagePercent = totalBytes > 0 ? 100.0 * usedBytes / totalBytes : 0.0;

            _logger.Debug("Storage info for {Path}: {UsedBytes} / {TotalBytes} ({Percent:F1}%)",
                path, usedBytes, totalBytes, usagePercent);

            return new StorageInfo
            {
                TotalBytes = totalBytes,
                UsedBytes = usedBytes,
                AvailableBytes = availableBytes,
                UsagePercent = usagePercent
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get storage info for path {Path}. Exception: {Message}", 
                path ?? "null", ex.Message);
            return new StorageInfo
            {
                TotalBytes = 0,
                UsedBytes = 0,
                AvailableBytes = 0,
                UsagePercent = 0.0
            };
        }
    }

    #region P/Invoke Declarations

    /// <summary>
    /// Structure representing filesystem statistics from statvfs().
    /// Must match the Linux x86_64 statvfs struct layout exactly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct StatVfs
    {
        public ulong f_bsize;    // Filesystem block size
        public ulong f_frsize;   // Fragment size
        public ulong f_blocks;   // Size of fs in f_frsize units
        public ulong f_bfree;    // Number of free blocks
        public ulong f_bavail;   // Number of free blocks for unprivileged users
        public ulong f_files;    // Number of inodes
        public ulong f_ffree;    // Number of free inodes
        public ulong f_favail;   // Number of free inodes for unprivileged users
        public ulong f_fsid;     // Filesystem ID
        public ulong f_flag;     // Mount flags
        public ulong f_namemax;  // Maximum filename length
        public uint f_type;      // Filesystem type
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] __f_spare;  // Padding/reserved fields
    }

    /// <summary>
    /// P/Invoke declaration for statvfs() system call.
    /// </summary>
    /// <param name="path">Path to check storage for.</param>
    /// <param name="buf">Buffer to receive filesystem statistics.</param>
    /// <returns>0 on success, -1 on error.</returns>
    [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern int statvfs(string path, ref StatVfs buf);

    #endregion
}
