using RunLoki365.Interfaces;
using RunLoki365.Models;
using Serilog;

namespace RunLoki365.Runners;

/// <summary>
/// Simple runner implementation using embedded PNG resources.
/// </summary>
public class EmbeddedResourceRunner : IRunner
{
    private readonly ILogger _logger;
    private readonly Dictionary<Theme, Gdk.Pixbuf[]> _frames;

    public string Name { get; }
    public int FrameCount { get; }

    public EmbeddedResourceRunner(string name, int frameCount, ILogger logger)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FrameCount = frameCount;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _frames = new Dictionary<Theme, Gdk.Pixbuf[]>();
    }

    /// <inheritdoc/>
    public Gdk.Pixbuf GetFrame(int index, Theme theme)
    {
        if (index < 0 || index >= FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Frame index must be between 0 and {FrameCount - 1}");
        }

        // Lazy load frames for the requested theme
        if (!_frames.ContainsKey(theme))
        {
            LoadFrames(theme);
        }

        return _frames[theme][index];
    }

    private void LoadFrames(Theme theme)
    {
        try
        {
            var frames = new Gdk.Pixbuf[FrameCount];
            var themePrefix = theme == Theme.Dark ? "dark" : "light";
            var assembly = typeof(EmbeddedResourceRunner).Assembly;

            for (int i = 0; i < FrameCount; i++)
            {
                // Build resource name: RunLoki365.Resources.runners.{animal}.{theme}_{animal}_{frame}.png
                var resourceName = $"RunLoki365.Resources.runners.{Name.ToLower()}.{themePrefix}_{Name.ToLower()}_{i}.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        _logger.Warning("Embedded resource not found: {ResourceName}, using fallback", resourceName);
                        frames[i] = CreateFallbackPixbuf(32, 32, theme);
                        continue;
                    }

                    frames[i] = new Gdk.Pixbuf(stream);
                    _logger.Debug("Loaded frame {Index} from {ResourceName}", i, resourceName);
                }
            }

            _frames[theme] = frames;
            _logger.Information("Loaded {Count} frames for runner {Name} ({Theme})", FrameCount, Name, theme);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load frames for runner {Name} ({Theme})", Name, theme);
            throw;
        }
    }

    private Gdk.Pixbuf CreateFallbackPixbuf(int width, int height, Theme theme)
    {
        // Create a simple colored square as fallback
        var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, height);

        // Fill with color based on runner and theme
        byte r = 200, g = 200, b = 200;
        if (Name.Equals("cat", StringComparison.OrdinalIgnoreCase))
        {
            r = theme == Theme.Dark ? (byte)255 : (byte)100;
            g = theme == Theme.Dark ? (byte)200 : (byte)50;
            b = theme == Theme.Dark ? (byte)100 : (byte)50;
        }
        else if (Name.Equals("horse", StringComparison.OrdinalIgnoreCase))
        {
            r = theme == Theme.Dark ? (byte)200 : (byte)80;
            g = theme == Theme.Dark ? (byte)150 : (byte)50;
            b = theme == Theme.Dark ? (byte)100 : (byte)30;
        }
        else if (Name.Equals("parrot", StringComparison.OrdinalIgnoreCase))
        {
            r = theme == Theme.Dark ? (byte)100 : (byte)50;
            g = theme == Theme.Dark ? (byte)255 : (byte)150;
            b = theme == Theme.Dark ? (byte)100 : (byte)50;
        }

        pixbuf.Fill((uint)((r << 24) | (g << 16) | (b << 8) | 0xFF));
        return pixbuf;
    }
}
