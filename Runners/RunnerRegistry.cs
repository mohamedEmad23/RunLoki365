using RunLoki365.Interfaces;
using Serilog;

namespace RunLoki365.Runners;

/// <summary>
/// Registry of available runner characters.
/// </summary>
public class RunnerRegistry
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, IRunner> _runners;

    public RunnerRegistry(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _runners = new Dictionary<string, IRunner>(StringComparer.OrdinalIgnoreCase);

        RegisterBuiltInRunners();
    }

    /// <summary>
    /// Gets a runner by name.
    /// </summary>
    public IRunner GetRunner(string name)
    {
        if (_runners.TryGetValue(name, out var runner))
        {
            return runner;
        }

        _logger.Warning("Runner {Name} not found, returning default", name);
        return _runners["Cat"]; // Default fallback
    }

    /// <summary>
    /// Gets all available runner names.
    /// </summary>
    public IEnumerable<string> GetAllRunnerNames()
    {
        return _runners.Keys;
    }

    /// <summary>
    /// Registers a custom runner.
    /// </summary>
    public void RegisterRunner(IRunner runner)
    {
        _runners[runner.Name] = runner;
        _logger.Information("Registered runner: {Name}", runner.Name);
    }

    private void RegisterBuiltInRunners()
    {
        // Register built-in runners with frame counts matching actual image files
        RegisterRunner(new EmbeddedResourceRunner("Cat", 5, _logger));
        RegisterRunner(new EmbeddedResourceRunner("Horse", 14, _logger));
        RegisterRunner(new EmbeddedResourceRunner("Parrot", 10, _logger));

        _logger.Information("Registered {Count} built-in runners", _runners.Count);
    }
}
