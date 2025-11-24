using Gdk;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for AppIndicator system tray integration.
/// </summary>
public interface IAppIndicatorService : IDisposable
{
    /// <summary>
    /// Initializes the AppIndicator.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Updates the system tray icon.
    /// </summary>
    /// <param name="pixbuf">New icon image.</param>
    void UpdateIcon(Pixbuf pixbuf);

    /// <summary>
    /// Updates the tooltip text.
    /// </summary>
    /// <param name="text">Tooltip text to display.</param>
    void UpdateTooltip(string text);

    /// <summary>
    /// Shows the context menu.
    /// </summary>
    void ShowMenu();

    /// <summary>
    /// Sets the context menu for the indicator.
    /// </summary>
    /// <param name="menu">GTK menu to display.</param>
    void SetMenu(Gtk.Menu menu);
}
