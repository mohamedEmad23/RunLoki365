# Changelog

All notable changes to RunLoki365 will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-11-24

### Added
- Initial release of RunLoki365 for Linux
- Real-time system monitoring
  - CPU usage tracking via `/proc/stat` with exponential smoothing
  - Memory monitoring via `/proc/meminfo`
  - Storage monitoring via `statvfs()` system call
- Animated tray icon with 3 built-in runners:
  - Tux (Linux penguin)
  - Cat
  - Parrot
- Animation system
  - Adaptive FPS (10-40 FPS) based on CPU usage
  - Configurable FPS limits (15/30/60/Unlimited)
  - Current FPS options [10, 20, 30, 40]
  - Smooth GLib-based animation loop
- Theme management
  - Automatic light/dark theme detection via GTK settings
  - Manual theme override support
- Settings management
  - JSON-based configuration at `~/.config/runloki365/settings.json`
  - Atomic file writes with crash safety
  - Schema versioning for future compatibility
- AppIndicator integration
  - Native Ayatana AppIndicator support via P/Invoke
  - Context menu with About dialog
  - Right-click menu integration
- Startup management
  - XDG autostart support
  - Desktop file generation at `~/.config/autostart/`
- Application lifecycle
  - Dependency injection with Microsoft.Extensions.DependencyInjection
  - Structured logging with Serilog
  - Crash report generation
  - Signal handling (SIGINT, SIGTERM, Ctrl+C)
  - Graceful shutdown with cleanup
- Error handling
  - Global exception handlers
  - Critical dependency validation on startup
  - Desktop notifications for critical errors (via notify-send)
  - Comprehensive logging (INFO/ERROR/DEBUG levels)
- Version information
  - Semantic versioning (1.0.0 "Loki")
  - Build date and commit hash tracking
  - About dialog with version details
- Packaging
  - Self-contained .NET 9.0 build (74 MB executable)
  - Single-file deployment
  - Debian package (.deb) with dpkg support
  - Install/uninstall scripts
  - Desktop file for application menu

### Technical Details
- Built with .NET 9.0
- GTK# 3.24.24.95 bindings
- Ayatana AppIndicator library (P/Invoke)
- Clean architecture with 4 layers:
  - Interfaces (contracts)
  - Models (domain objects)
  - Services (business logic)
  - Monitors (system integration)
- Zero build warnings, zero runtime errors
- Production-ready error handling and logging

### Known Issues
- AppIndicator tooltips not supported (library limitation)
- GNOME Shell requires AppIndicator extension
- Single-file publish disables trimming (reflection usage in JSON serialization)

## [Unreleased]

### Planned
- Preferences dialog UI
- Additional runner animations
- Network monitoring
- GPU monitoring (NVIDIA/AMD)
- Localization (i18n) support
- Custom runner support (load from PNG files)
- Snap/Flatpak packaging
- AUR package for Arch Linux
