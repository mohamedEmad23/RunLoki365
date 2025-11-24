using Serilog;
using Serilog.Events;

namespace RunLoki365.Logging;

/// <summary>
/// Configures Serilog logging for the application.
/// </summary>
public static class LoggerSetup
{
    /// <summary>
    /// Configures and returns a Serilog logger instance.
    /// </summary>
    /// <returns>Configured logger.</returns>
    public static ILogger ConfigureLogger()
    {
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "runloki365",
            "logs"
        );

        Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, "runloki365.log");

        return new Serilog.LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Infinite,
                fileSizeLimitBytes: 5_242_880, // 5 MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 3,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();
    }
}
