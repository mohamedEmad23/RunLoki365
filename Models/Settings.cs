namespace RunLoki365.Models;

/// <summary>
/// Application settings model.
/// </summary>
public class Settings
{
    /// <summary>
    /// Settings schema version for future migration support.
    /// </summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>
    /// Selected runner name (e.g., "Cat", "Horse", "Parrot").
    /// </summary>
    public string Runner { get; set; } = "Cat";

    /// <summary>
    /// Theme preference ("SystemAuto", "Light", or "Dark").
    /// </summary>
    public string ThemeMode { get; set; } = "SystemAuto";

    /// <summary>
    /// Maximum FPS limit (10, 20, 30, or 40).
    /// </summary>
    public int FpsLimit { get; set; } = 30;

    /// <summary>
    /// Whether to launch at system startup.
    /// </summary>
    public bool LaunchAtStartup { get; set; } = false;
}
