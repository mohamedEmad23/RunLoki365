using System.Runtime.InteropServices;
using RunLoki365.Interfaces;
using Serilog;

namespace RunLoki365.Services;

/// <summary>
/// AppIndicator3 service implementation using Ayatana AppIndicator library via P/Invoke.
/// Manages the system tray icon and context menu.
/// </summary>
public class AppIndicator3Service : IAppIndicatorService
{
    private readonly ILogger _logger;
    private IntPtr _indicator;
    private IntPtr _menu;
    private bool _isInitialized;
    private string _iconTempPath;
    private int _iconCounter = 0;

    public AppIndicator3Service(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Create temp directory for icon files
        var tempDir = Path.Combine(Path.GetTempPath(), "runloki365-icons");
        Directory.CreateDirectory(tempDir);
        _iconTempPath = tempDir;
    }

    /// <inheritdoc/>
    public void Initialize()
    {
        if (_isInitialized)
        {
            _logger.Warning("AppIndicator already initialized");
            return;
        }

        try
        {
            _logger.Information("Initializing AppIndicator");

            // Create AppIndicator
            // Parameters: id, icon_name, category
            _indicator = app_indicator_new(
                "runloki365",
                "system-run", // Default icon (will be updated later)
                AppIndicatorCategory.SystemServices
            );

            if (_indicator == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create AppIndicator instance");
            }

            // Set status to active (visible)
            app_indicator_set_status(_indicator, AppIndicatorStatus.Active);

            // Create an empty menu (will be populated later)
            _menu = gtk_menu_new();
            app_indicator_set_menu(_indicator, _menu);

            _isInitialized = true;
            _logger.Information("AppIndicator initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize AppIndicator");
            throw;
        }
    }

    /// <inheritdoc/>
    public void UpdateIcon(Gdk.Pixbuf pixbuf)
    {
        if (!_isInitialized)
        {
            _logger.Warning("Cannot update icon: AppIndicator not initialized");
            return;
        }

        try
        {
            // AppIndicator requires icon files, not pixbufs
            // Save the pixbuf to a temp file and update the icon path
            var iconPath = Path.Combine(_iconTempPath, $"icon_{_iconCounter}.png");
            
            // Save pixbuf to PNG file (null arrays for default PNG options)
            pixbuf.Savev(iconPath, "png", new string[0], new string[0]);
            
            // Update AppIndicator icon
            app_indicator_set_icon_full(_indicator, iconPath, "RunLoki365");
            
            // Clean up old icon file (keep last 2 to avoid race conditions)
            if (_iconCounter > 1)
            {
                var oldIconPath = Path.Combine(_iconTempPath, $"icon_{_iconCounter - 2}.png");
                if (File.Exists(oldIconPath))
                {
                    try { File.Delete(oldIconPath); } catch { }
                }
            }
            
            _iconCounter++;
            _logger.Debug("Icon updated to frame {Counter}", _iconCounter);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update icon");
        }
    }

    /// <inheritdoc/>
    public void UpdateTooltip(string text)
    {
        if (!_isInitialized)
        {
            _logger.Warning("Cannot update tooltip: AppIndicator not initialized");
            return;
        }

        try
        {
            // AppIndicator doesn't support tooltips directly
            // Tooltips are shown via menu items instead
            _logger.Debug("Tooltip update: {Tooltip}", text);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update tooltip");
        }
    }

    /// <inheritdoc/>
    public void ShowMenu()
    {
        if (!_isInitialized)
        {
            _logger.Warning("Cannot show menu: AppIndicator not initialized");
            return;
        }

        // Menu is automatically shown on click by AppIndicator
        _logger.Debug("Menu display managed by AppIndicator");
    }

    /// <inheritdoc/>
    public void SetMenu(Gtk.Menu menu)
    {
        if (!_isInitialized)
        {
            _logger.Warning("Cannot set menu: AppIndicator not initialized");
            return;
        }

        try
        {
            _menu = menu.Handle;
            app_indicator_set_menu(_indicator, _menu);
            _logger.Debug("Menu updated");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set menu");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_indicator != IntPtr.Zero)
        {
            // AppIndicator doesn't need explicit cleanup
            // GTK will handle it when the application exits
            _indicator = IntPtr.Zero;
            
            // Clean up temp icon files
            try
            {
                if (Directory.Exists(_iconTempPath))
                {
                    Directory.Delete(_iconTempPath, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to clean up temp icon directory");
            }
            
            _logger.Information("AppIndicator disposed");
        }
    }

    #region P/Invoke Declarations for Ayatana AppIndicator

    private enum AppIndicatorCategory
    {
        ApplicationStatus = 0,
        Communications = 1,
        SystemServices = 2,
        Hardware = 3,
        Other = 4
    }

    private enum AppIndicatorStatus
    {
        Passive = 0,
        Active = 1,
        Attention = 2
    }

    [DllImport("libayatana-appindicator3.so.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr app_indicator_new(
        string id,
        string icon_name,
        AppIndicatorCategory category
    );

    [DllImport("libayatana-appindicator3.so.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern void app_indicator_set_status(
        IntPtr self,
        AppIndicatorStatus status
    );

    [DllImport("libayatana-appindicator3.so.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern void app_indicator_set_menu(
        IntPtr self,
        IntPtr menu
    );

    [DllImport("libayatana-appindicator3.so.1", CallingConvention = CallingConvention.Cdecl)]
    private static extern void app_indicator_set_icon_full(
        IntPtr self,
        string icon_name,
        string icon_desc
    );

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_menu_new();

    #endregion
}
