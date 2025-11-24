using RunLoki365.Interfaces;
using RunLoki365.Runners;
using Serilog;

namespace RunLoki365.Services;

/// <summary>
/// Main application controller that orchestrates all services.
/// </summary>
public class ApplicationController
{
    private readonly ILogger _logger;
    private readonly ISystemMonitor _systemMonitor;
    private readonly IAnimationManager _animationManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IAppIndicatorService _appIndicator;
    private readonly IThemeManager _themeManager;
    private readonly IStartupManager _startupManager;
    private readonly RunnerRegistry _runnerRegistry;
    private uint _tooltipTimerId;

    public ApplicationController(
        ILogger logger,
        ISystemMonitor systemMonitor,
        IAnimationManager animationManager,
        ISettingsManager settingsManager,
        IAppIndicatorService appIndicator,
        IThemeManager themeManager,
        IStartupManager startupManager,
        RunnerRegistry runnerRegistry)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _systemMonitor = systemMonitor ?? throw new ArgumentNullException(nameof(systemMonitor));
        _animationManager = animationManager ?? throw new ArgumentNullException(nameof(animationManager));
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _appIndicator = appIndicator ?? throw new ArgumentNullException(nameof(appIndicator));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _startupManager = startupManager ?? throw new ArgumentNullException(nameof(startupManager));
        _runnerRegistry = runnerRegistry ?? throw new ArgumentNullException(nameof(runnerRegistry));
    }

    /// <summary>
    /// Starts the application.
    /// </summary>
    public void Start()
    {
        try
        {
            _logger.Information("Starting ApplicationController");

            // Load settings
            var settings = _settingsManager.Load();
            _logger.Information("Settings loaded: Runner={Runner}, ThemeMode={ThemeMode}, FpsLimit={FpsLimit}",
                settings.Runner, settings.ThemeMode, settings.FpsLimit);

            // Apply theme override
            if (Enum.TryParse<Models.ThemeMode>(settings.ThemeMode, out var themeMode))
            {
                _themeManager.SetThemeOverride(themeMode);
            }

            // Set up runner
            var runner = _runnerRegistry.GetRunner(settings.Runner);
            _animationManager.SetRunner(runner);
            _animationManager.SetFpsLimit(settings.FpsLimit);

            // Initialize AppIndicator
            _appIndicator.Initialize();

            // Create and set context menu
            var menu = BuildContextMenu();
            _appIndicator.SetMenu(menu);

            // Wire up CPU usage event
            _systemMonitor.CpuUsageChanged += OnCpuUsageChanged;

            // Start monitoring
            _systemMonitor.StartMonitoring();

            // Start animation
            _animationManager.StartAnimation();

            // Start tooltip update timer (every 1 second)
            _tooltipTimerId = GLib.Timeout.Add(1000, OnTooltipUpdate);

            _logger.Information("ApplicationController started successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start ApplicationController");
            throw;
        }
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public void Stop()
    {
        try
        {
            _logger.Information("Stopping ApplicationController");

            // Stop tooltip timer
            if (_tooltipTimerId != 0)
            {
                GLib.Source.Remove(_tooltipTimerId);
                _tooltipTimerId = 0;
            }

            // Stop animation
            _animationManager.StopAnimation();

            // Stop monitoring
            _systemMonitor.StopMonitoring();

            // Save settings
            var settings = _settingsManager.CurrentSettings;
            _settingsManager.Save(settings);

            _logger.Information("ApplicationController stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error stopping ApplicationController");
        }
    }

    private void OnCpuUsageChanged(object? sender, double cpuUsage)
    {
        try
        {
            // Update animation speed based on CPU usage
            _animationManager.UpdateAnimationSpeed(cpuUsage);

            // Update icon with current frame
            var frame = _animationManager.GetCurrentFrame();
            if (frame != null)
            {
                _appIndicator.UpdateIcon(frame);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling CPU usage change");
        }
    }

    private bool OnTooltipUpdate()
    {
        try
        {
            var cpu = _systemMonitor.GetCpuUsage();
            var memory = _systemMonitor.GetMemoryInfo();
            var storage = _systemMonitor.GetStorageInfo();

            var tooltip = $"CPU: {cpu:F1}%\n" +
                         $"Memory: {memory.FormatUsed()} / {memory.FormatTotal()} ({memory.UsagePercent:F1}%)\n" +
                         $"Storage: {storage.FormatUsed()} / {storage.FormatTotal()} ({storage.UsagePercent:F1}%)";

            _appIndicator.UpdateTooltip(tooltip);
            return true; // Continue timer
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating tooltip");
            return true; // Continue despite error
        }
    }

    private Gtk.Menu BuildContextMenu()
    {
        var menu = new Gtk.Menu();
        var settings = _settingsManager.CurrentSettings;

        // Runners submenu
        var runnersItem = new Gtk.MenuItem("Runners");
        var runnersMenu = new Gtk.Menu();
        
        foreach (var runnerName in _runnerRegistry.GetAllRunnerNames())
        {
            var runnerMenuItem = new Gtk.CheckMenuItem(runnerName)
            {
                Active = settings.Runner.Equals(runnerName, StringComparison.OrdinalIgnoreCase)
            };
            runnerMenuItem.Activated += (s, e) => ChangeRunner(runnerName);
            runnersMenu.Append(runnerMenuItem);
        }
        
        runnersItem.Submenu = runnersMenu;
        menu.Append(runnersItem);

        // Theme submenu
        var themeItem = new Gtk.MenuItem("Theme");
        var themeMenu = new Gtk.Menu();
        
        var themes = new[] { "SystemAuto", "Light", "Dark" };
        foreach (var themeName in themes)
        {
            var themeMenuItem = new Gtk.CheckMenuItem(themeName)
            {
                Active = settings.ThemeMode.Equals(themeName, StringComparison.OrdinalIgnoreCase)
            };
            themeMenuItem.Activated += (s, e) => ChangeTheme(themeName);
            themeMenu.Append(themeMenuItem);
        }
        
        themeItem.Submenu = themeMenu;
        menu.Append(themeItem);

        // FPS submenu
        var fpsItem = new Gtk.MenuItem("FPS Limit");
        var fpsMenu = new Gtk.Menu();
        
        var fpsLimits = new[] { 10, 20, 30, 40 };
        foreach (var fps in fpsLimits)
        {
            var fpsMenuItem = new Gtk.CheckMenuItem($"{fps} FPS")
            {
                Active = settings.FpsLimit == fps
            };
            fpsMenuItem.Activated += (s, e) => ChangeFpsLimit(fps);
            fpsMenu.Append(fpsMenuItem);
        }
        
        fpsItem.Submenu = fpsMenu;
        menu.Append(fpsItem);

        // Separator
        menu.Append(new Gtk.SeparatorMenuItem());

        // Launch at startup
        var startupItem = new Gtk.CheckMenuItem("Launch at Startup")
        {
            Active = settings.LaunchAtStartup
        };
        startupItem.Activated += (s, e) => ToggleStartup();
        menu.Append(startupItem);

        // Separator
        menu.Append(new Gtk.SeparatorMenuItem());

        // About menu item
        var aboutItem = new Gtk.MenuItem("About RunLoki365");
        aboutItem.Activated += (s, e) => ShowAboutDialog();
        menu.Append(aboutItem);

        // Separator
        menu.Append(new Gtk.SeparatorMenuItem());

        // Exit menu item
        var exitItem = new Gtk.MenuItem("Exit");
        exitItem.Activated += (s, e) =>
        {
            _logger.Information("Exit requested from menu");
            // Stop and quit immediately without waiting for idle
            Task.Run(() =>
            {
                Stop();
                GLib.Idle.Add(() =>
                {
                    Gtk.Application.Quit();
                    return false;
                });
            });
        };
        menu.Append(exitItem);

        menu.ShowAll();
        return menu;
    }

    private void ChangeRunner(string runnerName)
    {
        try
        {
            _logger.Information("Changing runner to {Runner}", runnerName);
            var runner = _runnerRegistry.GetRunner(runnerName);
            _animationManager.SetRunner(runner);
            
            var settings = _settingsManager.CurrentSettings;
            settings.Runner = runnerName;
            _settingsManager.Save(settings);
            
            // Rebuild menu to update checkmarks
            var menu = BuildContextMenu();
            _appIndicator.SetMenu(menu);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to change runner");
        }
    }

    private void ChangeTheme(string themeName)
    {
        try
        {
            _logger.Information("Changing theme to {Theme}", themeName);
            
            if (Enum.TryParse<Models.ThemeMode>(themeName, out var themeMode))
            {
                _themeManager.SetThemeOverride(themeMode);
                
                // Reload runner frames for new theme
                var settings = _settingsManager.CurrentSettings;
                settings.ThemeMode = themeName;
                _settingsManager.Save(settings);
                
                var runner = _runnerRegistry.GetRunner(settings.Runner);
                _animationManager.SetRunner(runner);
                
                // Rebuild menu to update checkmarks
                var menu = BuildContextMenu();
                _appIndicator.SetMenu(menu);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to change theme");
        }
    }

    private void ChangeFpsLimit(int fps)
    {
        try
        {
            _logger.Information("Changing FPS limit to {FPS}", fps);
            _animationManager.SetFpsLimit(fps);
            
            var settings = _settingsManager.CurrentSettings;
            settings.FpsLimit = fps;
            _settingsManager.Save(settings);
            
            // Rebuild menu to update checkmarks
            var menu = BuildContextMenu();
            _appIndicator.SetMenu(menu);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to change FPS limit");
        }
    }

    private void ToggleStartup()
    {
        try
        {
            var settings = _settingsManager.CurrentSettings;
            var newValue = !settings.LaunchAtStartup;
            
            _logger.Information("Toggling startup: {Value}", newValue);
            
            if (newValue)
            {
                _startupManager.EnableStartup();
            }
            else
            {
                _startupManager.DisableStartup();
            }
            
            settings.LaunchAtStartup = newValue;
            _settingsManager.Save(settings);
            
            // Rebuild menu to update checkmarks
            var menu = BuildContextMenu();
            _appIndicator.SetMenu(menu);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to toggle startup");
        }
    }

    private void ShowAboutDialog()
    {
        try
        {
            var dialog = new Gtk.AboutDialog
            {
                ProgramName = "RunLoki365",
                Version = Models.VersionInfo.FullVersion,
                Comments = "System monitor with animated tray icon for Linux",
                Copyright = Models.VersionInfo.Copyright,
                License = $"{Models.VersionInfo.License} License",
                Website = Models.VersionInfo.GitHubUrl,
                WebsiteLabel = "GitHub Repository",
                Authors = new[] { "mohamedEmad23" },
                LogoIconName = "system-run",
                Modal = true,
                TransientFor = null
            };

            // Add build information to comments
            var extendedInfo = $@"System monitor with animated tray icon for Linux

Build: {Models.VersionInfo.BuildDate}
Commit: {Models.VersionInfo.CommitHash}
{(Models.VersionInfo.IsDirtyBuild ? "(dirty build)" : "")}

Monitors CPU, memory, and storage usage with customizable animated runners.";

            dialog.Comments = extendedInfo;

            dialog.Run();
            dialog.Destroy();

            _logger.Information("About dialog shown");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to show About dialog");
        }
    }
}
