namespace WorkflowEngine.Modules;

public class ModulesResponse
{
    public IReadOnlyCollection<ModuleManifest> Events  { get; set; } = [];
    public IReadOnlyCollection<ModuleManifest> Actions { get; set; } = [];
}

/// <summary>
/// Central registry of all registered modules.
/// Adding a new module = implement the interface + 1 DI line in Program.cs.
/// </summary>
public class ModuleRegistry(
    IEnumerable<IEventModule>  events,
    IEnumerable<IActionModule> actions)
{
    private readonly Dictionary<string, IEventModule>  _events  = events.ToDictionary(m => m.ModuleId);
    private readonly Dictionary<string, IActionModule> _actions = actions.ToDictionary(m => m.ModuleId);

    public IEventModule?  GetEvent(string moduleId)  => _events.GetValueOrDefault(moduleId);
    public IActionModule? GetAction(string moduleId) => _actions.GetValueOrDefault(moduleId);

    public IEnumerable<IEventModule>  AllEvents  => _events.Values;
    public IEnumerable<IActionModule> AllActions => _actions.Values;

    // Dynamic module management (for custom modules loaded at runtime)
    public void AddDynamic(IEventModule m)  => _events[m.ModuleId]  = m;
    public void AddDynamic(IActionModule m) => _actions[m.ModuleId] = m;

    public void RemoveDynamic(string moduleId)
    {
        _events.Remove(moduleId);
        _actions.Remove(moduleId);
    }

    public ModulesResponse GetAllManifests() => new()
    {
        Events  = _events.Values.Select(m => m.Manifest).ToList(),
        Actions = _actions.Values.Select(m => m.Manifest).ToList()
    };
}
