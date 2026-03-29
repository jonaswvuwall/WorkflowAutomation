namespace WorkflowEngine.Modules;

/// <summary>
/// Central registry of all registered modules.
/// Adding a new module = implement the interface + 1 DI line in Program.cs.
/// </summary>
public class ModuleRegistry(
    IEnumerable<IEventModule>     events,
    IEnumerable<IConditionModule> conditions,
    IEnumerable<IActionModule>    actions)
{
    private readonly Dictionary<string, IEventModule>     _events     = events.ToDictionary(m => m.ModuleId);
    private readonly Dictionary<string, IConditionModule> _conditions = conditions.ToDictionary(m => m.ModuleId);
    private readonly Dictionary<string, IActionModule>    _actions    = actions.ToDictionary(m => m.ModuleId);

    public IEventModule?     GetEvent(string moduleId)     => _events.GetValueOrDefault(moduleId);
    public IConditionModule? GetCondition(string moduleId) => _conditions.GetValueOrDefault(moduleId);
    public IActionModule?    GetAction(string moduleId)    => _actions.GetValueOrDefault(moduleId);

    public IEnumerable<IEventModule>     AllEvents     => _events.Values;
    public IEnumerable<IConditionModule> AllConditions => _conditions.Values;
    public IEnumerable<IActionModule>    AllActions    => _actions.Values;

    public ModulesResponse GetAllManifests() => new()
    {
        Events     = _events.Values.Select(m => m.Manifest).ToList(),
        Conditions = _conditions.Values.Select(m => m.Manifest).ToList(),
        Actions    = _actions.Values.Select(m => m.Manifest).ToList()
    };
}
