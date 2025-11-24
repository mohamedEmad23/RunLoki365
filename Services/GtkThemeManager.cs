using RunLoki365.Interfaces;
using RunLoki365.Models;
using Serilog;

namespace RunLoki365.Services;

/// <summary>
/// GTK theme manager implementation.
/// Detects system theme and manages theme overrides.
/// </summary>
public class GtkThemeManager : IThemeManager
{
    private readonly ILogger _logger;
    private ThemeMode _themeMode = ThemeMode.SystemAuto;
    private Theme _currentTheme = Theme.Light;

    /// <inheritdoc/>
    public event EventHandler<Theme>? ThemeChanged;

    public GtkThemeManager(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        DetectCurrentTheme();
    }

    /// <inheritdoc/>
    public Theme GetCurrentTheme()
    {
        if (_themeMode != ThemeMode.SystemAuto)
        {
            // Return override theme
            return _themeMode == ThemeMode.Dark ? Theme.Dark : Theme.Light;
        }

        return DetectSystemTheme();
    }

    /// <inheritdoc/>
    public void SetThemeOverride(ThemeMode mode)
    {
        _logger.Information("Setting theme override to {Mode}", mode);
        _themeMode = mode;

        var newTheme = GetCurrentTheme();
        if (newTheme != _currentTheme)
        {
            _currentTheme = newTheme;
            ThemeChanged?.Invoke(this, _currentTheme);
        }
    }

    /// <inheritdoc/>
    public ThemeMode GetThemeMode()
    {
        return _themeMode;
    }

    private void DetectCurrentTheme()
    {
        _currentTheme = DetectSystemTheme();
        _logger.Information("Detected system theme: {Theme}", _currentTheme);
    }

    private Theme DetectSystemTheme()
    {
        try
        {
            var gtkSettings = Gtk.Settings.Default;
            if (gtkSettings == null)
            {
                _logger.Warning("GTK settings not available, defaulting to Light theme");
                return Theme.Light;
            }

            // Try to get the gtk-application-prefer-dark-theme property
            var prefersDark = false;
            try
            {
                var value = gtkSettings.GetProperty("gtk-application-prefer-dark-theme");
                if (value.Val is bool boolValue)
                {
                    prefersDark = boolValue;
                }
            }
            catch
            {
                // Property not available, default to light theme
            }

            return prefersDark ? Theme.Dark : Theme.Light;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to detect system theme, defaulting to Light");
            return Theme.Light;
        }
    }
}
