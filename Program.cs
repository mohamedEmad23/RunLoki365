using Microsoft.Extensions.DependencyInjection;
using RunLoki365.Interfaces;
using RunLoki365.Logging;
using RunLoki365.Models;
using RunLoki365.Monitors;
using RunLoki365.Runners;
using RunLoki365.Services;
using Serilog;
using System.Runtime.InteropServices;

namespace RunLoki365;

/// <summary>
/// Main program entry point.
/// </summary>
public class Program
{
    private static ILogger? _logger;
    private static ApplicationController? _appController;

    /// <summary>
    /// Application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
        try
        {
            // Configure logging
            _logger = LoggerSetup.ConfigureLogger();
            Log.Logger = _logger;

            _logger.Information("RunLoki365 v{Version} starting", VersionInfo.FullVersion);
            _logger.Information("Build: {BuildDate} (commit: {Commit})", VersionInfo.BuildDate, VersionInfo.CommitHash);
            if (VersionInfo.IsDirtyBuild)
            {
                _logger.Warning("Running dirty build with uncommitted changes");
            }

            // Set up exception handling
            SetupExceptionHandling();

            // Set up signal handling for clean shutdown
            SetupSignalHandlers();

            // Configure dependency injection
            var serviceProvider = ConfigureServices();

            // Validate critical dependencies
            ValidateCriticalDependencies(serviceProvider);

            // Initialize GTK
            Gtk.Application.Init();

            _logger.Information("GTK initialized successfully");

            // Get and start the application controller
            var app = serviceProvider.GetRequiredService<ApplicationController>();
            _appController = app;
            app.Start();

            _logger.Information("Application initialization complete");

            // Run GTK main loop
            Gtk.Application.Run();

            // Clean shutdown
            app.Stop();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Fatal error during application startup");
            WriteCrashReport(ex);
            Environment.Exit(1);
        }
        finally
        {
            _logger?.Information("Application shutting down");
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Configures dependency injection services.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register logger
        services.AddSingleton(_logger!);

        // Register notification service
        services.AddSingleton<NotificationService>();

        // Register monitors
        services.AddSingleton<ICpuMonitor, ProcStatCpuMonitor>();
        services.AddSingleton<IMemoryMonitor, ProcMemInfoMonitor>();
        services.AddSingleton<IStorageMonitor, StatvfsStorageMonitor>();
        services.AddSingleton<ISystemMonitor, SystemMonitor>();

        // Register runners
        services.AddSingleton<RunnerRegistry>();

        // Register services
        services.AddSingleton<IThemeManager, GtkThemeManager>();
        services.AddSingleton<IAnimationManager, AnimationManager>();
        services.AddSingleton<ISettingsManager, JsonSettingsManager>();
        services.AddSingleton<IAppIndicatorService, AppIndicator3Service>();
        services.AddSingleton<IStartupManager, XdgAutostartManager>();

        // Register application controller
        services.AddSingleton<ApplicationController>();

        _logger?.Information("Dependency injection configured");

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Sets up signal handlers for graceful shutdown.
    /// </summary>
    private static void SetupSignalHandlers()
    {
        // Handle Ctrl+C (SIGINT) and SIGTERM for clean shutdown
        Console.CancelKeyPress += (sender, e) =>
        {
            _logger?.Information("Received interrupt signal, shutting down gracefully");
            e.Cancel = true; // Prevent immediate termination
            _appController?.Stop();
            GLib.Idle.Add(() =>
            {
                Gtk.Application.Quit();
                return false;
            });
        };

        // For SIGTERM, we rely on the AppDomain.ProcessExit event
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            _logger?.Information("Process exiting, performing cleanup");
            _appController?.Stop();
        };

        _logger?.Information("Signal handlers configured");
    }

    /// <summary>
    /// Validates critical dependencies and shows error notifications if missing.
    /// </summary>
    private static void ValidateCriticalDependencies(IServiceProvider serviceProvider)
    {
        var notificationService = serviceProvider.GetRequiredService<NotificationService>();

        // Check if /proc/stat is readable (critical for CPU monitoring)
        if (!File.Exists("/proc/stat"))
        {
            var errorMsg = "Critical: /proc/stat not found. CPU monitoring will not work.";
            _logger?.Error(errorMsg);
            notificationService.ShowCriticalError("RunLoki365 Error", errorMsg);
        }
        else
        {
            try
            {
                File.ReadAllText("/proc/stat");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Critical: Cannot read /proc/stat - {ex.Message}";
                _logger?.Error(ex, errorMsg);
                notificationService.ShowCriticalError("RunLoki365 Error", errorMsg);
            }
        }

        // Check if /proc/meminfo is readable (critical for memory monitoring)
        if (!File.Exists("/proc/meminfo"))
        {
            var errorMsg = "Critical: /proc/meminfo not found. Memory monitoring will not work.";
            _logger?.Error(errorMsg);
            notificationService.ShowCriticalError("RunLoki365 Error", errorMsg);
        }
        else
        {
            try
            {
                File.ReadAllText("/proc/meminfo");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Critical: Cannot read /proc/meminfo - {ex.Message}";
                _logger?.Error(ex, errorMsg);
                notificationService.ShowCriticalError("RunLoki365 Error", errorMsg);
            }
        }

        // Verify AppIndicator library is available (but don't initialize yet)
        // Just check that the service can be created
        try
        {
            var appIndicatorService = serviceProvider.GetService<IAppIndicatorService>();
            if (appIndicatorService == null)
            {
                throw new InvalidOperationException("AppIndicator service not registered");
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Critical: AppIndicator service unavailable - {ex.Message}. Make sure libayatana-appindicator3-dev is installed.";
            _logger?.Error(ex, errorMsg);
            notificationService.ShowCriticalError("RunLoki365 Error", errorMsg);
            throw; // This is truly critical - can't run without AppIndicator
        }

        _logger?.Information("All critical dependencies validated successfully");
    }

    /// <summary>
    /// Sets up global exception handling.
    /// </summary>
    private static void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            _logger?.Error(exception, "Unhandled exception occurred");
            WriteCrashReport(exception!);
        };

        GLib.ExceptionManager.UnhandledException += args =>
        {
            var exception = args.ExceptionObject as Exception;
            _logger?.Error(exception, "Unhandled GLib exception occurred");
            WriteCrashReport(exception!);
        };
    }

    /// <summary>
    /// Writes a crash report to disk.
    /// </summary>
    private static void WriteCrashReport(Exception exception)
    {
        try
        {
            var crashDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "runloki365",
                "logs"
            );

            Directory.CreateDirectory(crashDirectory);

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var crashFilePath = Path.Combine(crashDirectory, $"crash-{timestamp}.log");

            var crashReport = $@"RunLoki365 Crash Report
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Version: {VersionInfo.FullVersion}
Build: {VersionInfo.BuildDate}
Commit: {VersionInfo.CommitHash}
Dirty: {VersionInfo.IsDirtyBuild}

Exception Type: {exception.GetType().FullName}
Message: {exception.Message}

Stack Trace:
{exception.StackTrace}

Inner Exception:
{exception.InnerException?.ToString() ?? "None"}
";

            File.WriteAllText(crashFilePath, crashReport);

            // Clean up old crash reports (keep only last 5)
            var crashFiles = Directory.GetFiles(crashDirectory, "crash-*.log")
                .OrderByDescending(f => f)
                .Skip(5);

            foreach (var file in crashFiles)
            {
                File.Delete(file);
            }
        }
        catch
        {
            // Ignore errors during crash report writing
        }
    }
}
