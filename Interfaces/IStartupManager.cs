namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for managing application launch at system startup.
/// </summary>
public interface IStartupManager
{
    /// <summary>
    /// Enables the application to launch at system startup.
    /// </summary>
    void EnableStartup();

    /// <summary>
    /// Disables the application from launching at system startup.
    /// </summary>
    void DisableStartup();

    /// <summary>
    /// Checks if autostart is currently enabled.
    /// </summary>
    /// <returns>True if autostart is enabled, false otherwise.</returns>
    bool IsEnabled();
}
