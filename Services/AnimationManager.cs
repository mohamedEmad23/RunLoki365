using RunLoki365.Interfaces;
using RunLoki365.Models;
using Serilog;

namespace RunLoki365.Services;

/// <summary>
/// Animation manager implementation.
/// Manages runner animations with FPS calculation based on CPU usage.
/// </summary>
public class AnimationManager : IAnimationManager, IDisposable
{
    private readonly ILogger _logger;
    private readonly IThemeManager _themeManager;
    private IRunner? _currentRunner;
    private int _currentFrame;
    private int _fpsLimit = 30;
    private int _currentFps = 10;
    private uint _timeoutId;
    private bool _isRunning;

    public AnimationManager(ILogger logger, IThemeManager themeManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
    }

    /// <inheritdoc/>
    public IRunner? CurrentRunner => _currentRunner;

    /// <inheritdoc/>
    public int CurrentFps => _currentFps;

    /// <inheritdoc/>
    public void SetRunner(IRunner runner)
    {
        _logger.Information("Setting runner to {Name}", runner?.Name ?? "null");
        _currentRunner = runner;
        _currentFrame = 0;
    }

    /// <inheritdoc/>
    public void SetFpsLimit(int limit)
    {
        if (limit != 10 && limit != 20 && limit != 30 && limit != 40)
        {
            throw new ArgumentException("FPS limit must be 10, 20, 30, or 40", nameof(limit));
        }

        _logger.Information("Setting FPS limit to {Limit}", limit);
        _fpsLimit = limit;
        UpdateAnimationSpeed(GetLastCpuUsage());
    }

    /// <inheritdoc/>
    public void UpdateAnimationSpeed(double cpuUsage)
    {
        // Calculate target FPS: 10 + (cpu_usage * 0.3)
        // At 0% CPU: 10 FPS
        // At 100% CPU: 40 FPS
        var calculatedFps = 10 + (cpuUsage * 0.3);
        var targetFps = (int)Math.Min(_fpsLimit, calculatedFps);

        if (targetFps != _currentFps)
        {
            _currentFps = targetFps;
            _logger.Debug("Animation speed updated: {Fps} FPS (CPU: {Cpu:F1}%)", _currentFps, cpuUsage);

            // Restart timer with new interval
            if (_isRunning)
            {
                StopAnimation();
                StartAnimation();
            }
        }

        _lastCpuUsage = cpuUsage;
    }

    /// <inheritdoc/>
    public Gdk.Pixbuf? GetCurrentFrame()
    {
        if (_currentRunner == null)
        {
            return null;
        }

        var theme = _themeManager.GetCurrentTheme();
        return _currentRunner.GetFrame(_currentFrame, theme);
    }

    /// <inheritdoc/>
    public void StartAnimation()
    {
        if (_isRunning)
        {
            _logger.Warning("Animation already running");
            return;
        }

        if (_currentRunner == null)
        {
            _logger.Warning("Cannot start animation: no runner set");
            return;
        }

        var intervalMs = _currentFps > 0 ? 1000 / _currentFps : 100;
        _timeoutId = GLib.Timeout.Add((uint)intervalMs, OnAnimationTick);
        _isRunning = true;

        _logger.Information("Animation started at {Fps} FPS ({Interval}ms interval)", _currentFps, intervalMs);
    }

    /// <inheritdoc/>
    public void StopAnimation()
    {
        if (!_isRunning)
        {
            return;
        }

        if (_timeoutId != 0)
        {
            GLib.Source.Remove(_timeoutId);
            _timeoutId = 0;
        }

        _isRunning = false;
        _logger.Information("Animation stopped");
    }

    private bool OnAnimationTick()
    {
        try
        {
            AdvanceFrame();

            // Update interval if FPS changed
            var expectedInterval = _currentFps > 0 ? 1000 / _currentFps : 100;
            return true; // Continue timer
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in animation tick");
            return true; // Continue despite error
        }
    }

    private void AdvanceFrame()
    {
        if (_currentRunner == null)
        {
            return;
        }

        _currentFrame = (_currentFrame + 1) % _currentRunner.FrameCount;
    }

    private double _lastCpuUsage = 0.0;

    private double GetLastCpuUsage()
    {
        return _lastCpuUsage;
    }

    public void Dispose()
    {
        StopAnimation();
    }
}
