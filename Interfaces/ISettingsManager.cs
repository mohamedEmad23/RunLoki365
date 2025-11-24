using RunLoki365.Models;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for managing application settings persistence.
/// </summary>
public interface ISettingsManager
{
    /// <summary>
    /// Gets the current settings.
    /// </summary>
    Settings CurrentSettings { get; }

    /// <summary>
    /// Loads settings from disk.
    /// </summary>
    /// <returns>Loaded settings or defaults if file doesn't exist.</returns>
    Settings Load();

    /// <summary>
    /// Saves settings to disk atomically.
    /// </summary>
    /// <param name="settings">Settings to save.</param>
    void Save(Settings settings);

    /// <summary>
    /// Gets default settings.
    /// </summary>
    /// <returns>Default settings configuration.</returns>
    Settings GetDefaultSettings();
}
