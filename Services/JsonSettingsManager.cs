using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using RunLoki365.Interfaces;
using RunLoki365.Models;
using Serilog;

namespace RunLoki365.Services;

/// <summary>
/// JSON-based settings manager implementation.
/// Persists settings to ~/.config/runloki365/settings.json with atomic writes.
/// </summary>
public class JsonSettingsManager : ISettingsManager
{
    private readonly ILogger _logger;
    private readonly string _settingsPath;
    private readonly string _configDirectory;
    private Settings _currentSettings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonSettingsManager(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Use XDG config directory: ~/.config/runloki365/
        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrEmpty(configHome))
        {
            configHome = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config"
            );
        }

        _configDirectory = Path.Combine(configHome, "runloki365");
        _settingsPath = Path.Combine(_configDirectory, "settings.json");

        _currentSettings = GetDefaultSettings();
    }

    /// <inheritdoc/>
    public Settings CurrentSettings => _currentSettings;

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Settings types are preserved")]
    public Settings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                _logger.Information("Settings file not found, using defaults: {Path}", _settingsPath);
                _currentSettings = GetDefaultSettings();
                Save(_currentSettings); // Create initial settings file
                return _currentSettings;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<Settings>(json, JsonOptions);

            if (settings == null)
            {
                _logger.Warning("Failed to deserialize settings, using defaults");
                _currentSettings = GetDefaultSettings();
                return _currentSettings;
            }

            // Validate schema version
            if (settings.SchemaVersion != "1.0")
            {
                _logger.Warning("Unknown schema version {Version}, using defaults", settings.SchemaVersion);
                _currentSettings = GetDefaultSettings();
                return _currentSettings;
            }

            _logger.Information("Settings loaded successfully");
            _currentSettings = settings;
            return _currentSettings;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load settings from {Path}, using defaults", _settingsPath);
            _currentSettings = GetDefaultSettings();
            return _currentSettings;
        }
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Settings types are preserved")]
    public void Save(Settings settings)
    {
        try
        {
            // Ensure config directory exists
            Directory.CreateDirectory(_configDirectory);

            // Serialize to JSON
            var json = JsonSerializer.Serialize(settings, JsonOptions);

            // Atomic save: write to temp file, then rename
            var tempPath = _settingsPath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _settingsPath, overwrite: true);

            _currentSettings = settings;
            _logger.Information("Settings saved successfully to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save settings to {Path}", _settingsPath);
            throw;
        }
    }

    /// <inheritdoc/>
    public Settings GetDefaultSettings()
    {
        return new Settings
        {
            SchemaVersion = "1.0",
            Runner = "Tux",
            ThemeMode = "SystemAuto",
            FpsLimit = 30,
            LaunchAtStartup = false
        };
    }
}
