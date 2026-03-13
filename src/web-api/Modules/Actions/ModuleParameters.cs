namespace WorkflowEngine.Modules.Actions;

/// <summary>Helper for accessing action config parameters with validation.</summary>
internal sealed class ModuleParameters(Dictionary<string, string> raw)
{
    private readonly Dictionary<string, string> _dict =
        new(raw, StringComparer.OrdinalIgnoreCase);

    public string Require(string key) =>
        _dict.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v
            : throw new InvalidOperationException($"Missing required parameter '{key}'");

    public string Get(string key, string defaultValue) =>
        _dict.TryGetValue(key, out var v) ? v : defaultValue;
}
