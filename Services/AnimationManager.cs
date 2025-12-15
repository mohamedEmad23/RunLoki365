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
    private int _fpsMultiplier = 3; // 1=slow, 2=medium, 3=fast, 4=very fast
    private int _currentFps = 10;
    private int _timerFps = 0; // The FPS the current timer was started with
    private uint _timeoutId;
    private bool _isRunning;
    private bool _restartPending = false;

    /// <inheritdoc/>
    public event EventHandler<Gdk.Pixbuf?>? FrameChanged;

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
        // Map old FPS values to new multiplier system:
        // 10 -> 1 (slow), 20 -> 2 (medium), 30 -> 3 (fast), 40 -> 4 (very fast)
        _fpsMultiplier = limit switch
        {
            10 => 1,
            20 => 2,
            30 => 3,
            40 => 4,
            _ => 3 // default to fast
        };

        _logger.Information("Setting FPS multiplier to {Multiplier} (from limit {Limit})", _fpsMultiplier, limit);
        UpdateAnimationSpeed(GetLastCpuUsage());
    }

    /// <inheritdoc/>
    public void UpdateAnimationSpeed(double cpuUsage)
    {
        // Aggressive FPS scaling for responsive animation
        // Base FPS ranges by multiplier:
        // 1 (slow):      7-30 FPS
        // 2 (medium):    9-60 FPS  
        // 3 (fast):      11-90 FPS
        // 4 (very fast): 13-120 FPS
        
        var minFps = 5 + (_fpsMultiplier * 2);  // 7, 9, 11, 13
        var maxFps = 30 * _fpsMultiplier;        // 30, 60, 90, 120
        
        var cpuFactor = Math.Clamp(cpuUsage, 0, 100) / 100.0;
        
        // Use exponential scaling for more dramatic speed increase at high CPU
        var exponentialFactor = Math.Pow(cpuFactor, 0.7); // Makes it ramp up faster
        var targetFps = (int)(minFps + exponentialFactor * (maxFps - minFps));
        
        targetFps = Math.Clamp(targetFps, minFps, maxFps);

        if (targetFps != _currentFps)
        {
            _logger.Debug("Animation speed updated: {Fps} FPS (CPU: {Cpu:F1}%, Multiplier: {Mult})", 
                targetFps, cpuUsage, _fpsMultiplier);
            _currentFps = targetFps;
            
            // If timer is running at different FPS, schedule a restart
            if (_isRunning && _timerFps != _currentFps && !_restartPending)
            {
                _restartPending = true;
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
        _timerFps = _currentFps; // Track what FPS this timer was started with
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

            // Check if FPS changed and we need to restart with new interval
            if (_restartPending)
            {
                _restartPending = false;
                
                // Stop current timer and start new one with updated interval
                // Return false to stop this timer, then start new one
                GLib.Idle.Add(() =>
                {
                    if (_timeoutId != 0)
                    {
                        // Timer already stopped by returning false
                    }
                    _timeoutId = 0;
                    _isRunning = false;
                    StartAnimation();
                    return false;
                });
                return false; // Stop current timer
            }
            
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
        
        // Fire event to notify subscribers of frame change
        var frame = GetCurrentFrame();
        FrameChanged?.Invoke(this, frame);
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
