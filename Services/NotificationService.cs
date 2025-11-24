using Serilog;

namespace RunLoki365.Services;

/// <summary>
/// Service for showing desktop notifications.
/// </summary>
public class NotificationService
{
    private readonly ILogger _logger;

    public NotificationService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Shows a critical error notification to the user.
    /// </summary>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification message.</param>
    public void ShowCriticalError(string title, string message)
    {
        try
        {
            _logger.Error("Critical error notification: {Title} - {Message}", title, message);

            // Use notify-send if available (standard on most Linux desktops)
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notify-send",
                    Arguments = $"--urgency=critical --icon=error \"{title}\" \"{message}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(1000); // Wait max 1 second
        }
        catch (Exception ex)
        {
            // Graceful degradation - just log if notification fails
            _logger.Warning(ex, "Failed to show desktop notification");
        }
    }

    /// <summary>
    /// Shows an information notification to the user.
    /// </summary>
    /// <param name="title">Notification title.</param>
    /// <param name="message">Notification message.</param>
    public void ShowInfo(string title, string message)
    {
        try
        {
            _logger.Information("Info notification: {Title} - {Message}", title, message);

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notify-send",
                    Arguments = $"--urgency=normal --icon=info \"{title}\" \"{message}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(1000);
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "Failed to show desktop notification");
        }
    }
}
