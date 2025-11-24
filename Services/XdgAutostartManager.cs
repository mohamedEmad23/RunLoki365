using RunLoki365.Interfaces;
using Serilog;

namespace RunLoki365.Services;

/// <summary>
/// XDG autostart manager implementation.
/// Manages launch at startup via ~/.config/autostart/ desktop files.
/// </summary>
public class XdgAutostartManager : IStartupManager
{
    private readonly ILogger _logger;
    private readonly string _autostartPath;
    private readonly string _desktopFileName = "runloki365.desktop";

    public XdgAutostartManager(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrEmpty(configHome))
        {
            configHome = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config"
            );
        }

        _autostartPath = Path.Combine(configHome, "autostart", _desktopFileName);
    }

    /// <inheritdoc/>
    public void EnableStartup()
    {
        try
        {
            var autostartDir = Path.GetDirectoryName(_autostartPath)!;
            Directory.CreateDirectory(autostartDir);

            // Get the executable path
            var execPath = GetExecutablePath();

            // Create .desktop file content
            var desktopContent = $@"[Desktop Entry]
Type=Application
Name=RunLoki365
Comment=Animated system monitor for GNOME
Exec={execPath}
Icon=system-run
Terminal=false
Categories=Utility;System;
X-GNOME-Autostart-enabled=true
";

            File.WriteAllText(_autostartPath, desktopContent);
            _logger.Information("Autostart enabled: {Path}", _autostartPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to enable autostart");
            throw;
        }
    }

    /// <inheritdoc/>
    public void DisableStartup()
    {
        try
        {
            if (File.Exists(_autostartPath))
            {
                File.Delete(_autostartPath);
                _logger.Information("Autostart disabled: {Path}", _autostartPath);
            }
            else
            {
                _logger.Debug("Autostart file does not exist, nothing to disable");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to disable autostart");
            throw;
        }
    }

    /// <inheritdoc/>
    public bool IsEnabled()
    {
        var exists = File.Exists(_autostartPath);
        _logger.Debug("Autostart status: {Enabled}", exists);
        return exists;
    }

    private string GetExecutablePath()
    {
        // Use the current process path
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(processPath))
        {
            return processPath;
        }

        // Fallback to assembly location
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, "runloki365");
        if (File.Exists(assemblyPath))
        {
            return assemblyPath;
        }

        // Last resort: assume installed location
        return "/usr/local/bin/runloki365";
    }
}
