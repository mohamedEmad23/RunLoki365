using RunLoki365.Models;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for managing UI theme detection and overrides.
/// </summary>
public interface IThemeManager
{
    /// <summary>
    /// Event fired when the theme changes.
    /// </summary>
    event EventHandler<Theme>? ThemeChanged;

    /// <summary>
    /// Gets the current active theme.
    /// </summary>
    /// <returns>Current theme (Light or Dark).</returns>
    Theme GetCurrentTheme();

    /// <summary>
    /// Sets a manual theme override.
    /// </summary>
    /// <param name="mode">Theme mode to apply.</param>
    void SetThemeOverride(ThemeMode mode);
}
