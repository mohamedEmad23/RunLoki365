using Gdk;
using RunLoki365.Models;

namespace RunLoki365.Interfaces;

/// <summary>
/// Interface for animated runner characters.
/// </summary>
public interface IRunner
{
    /// <summary>
    /// Gets the runner name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the total number of animation frames.
    /// </summary>
    int FrameCount { get; }

    /// <summary>
    /// Gets a specific animation frame.
    /// </summary>
    /// <param name="index">Frame index (0-based).</param>
    /// <param name="theme">Theme to use for frame selection.</param>
    /// <returns>Pixbuf containing the frame image.</returns>
    Pixbuf GetFrame(int index, Theme theme);
}
