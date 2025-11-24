using Gdk;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for managing runner animations.
/// </summary>
public interface IAnimationManager
{
    /// <summary>
    /// Gets the currently active runner.
    /// </summary>
    IRunner? CurrentRunner { get; }

    /// <summary>
    /// Gets the current frames per second.
    /// </summary>
    int CurrentFps { get; }

    /// <summary>
    /// Sets the active runner.
    /// </summary>
    /// <param name="runner">The runner to activate.</param>
    void SetRunner(IRunner runner);

    /// <summary>
    /// Sets the maximum FPS limit.
    /// </summary>
    /// <param name="limit">FPS limit (10, 20, 30, or 40).</param>
    void SetFpsLimit(int limit);

    /// <summary>
    /// Updates animation speed based on CPU usage.
    /// </summary>
    /// <param name="cpuUsage">CPU usage percentage (0-100).</param>
    void UpdateAnimationSpeed(double cpuUsage);

    /// <summary>
    /// Gets the current animation frame.
    /// </summary>
    /// <returns>Pixbuf containing the current frame.</returns>
    Pixbuf? GetCurrentFrame();

    /// <summary>
    /// Starts the animation loop.
    /// </summary>
    void StartAnimation();

    /// <summary>
    /// Stops the animation loop.
    /// </summary>
    void StopAnimation();
}
