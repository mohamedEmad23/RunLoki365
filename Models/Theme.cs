namespace RunLoki365.Models;

/// <summary>
/// Theme enumeration for light/dark mode.
/// </summary>
public enum Theme
{
    /// <summary>
    /// Light theme with dark icons.
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme with light icons.
    /// </summary>
    Dark
}

/// <summary>
/// Theme mode enumeration for user preference.
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// Automatically detect from system settings.
    /// </summary>
    SystemAuto,

    /// <summary>
    /// Force light theme.
    /// </summary>
    Light,

    /// <summary>
    /// Force dark theme.
    /// </summary>
    Dark
}
