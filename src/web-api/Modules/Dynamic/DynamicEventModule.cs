using WorkflowEngine.Models;

namespace WorkflowEngine.Modules.Dynamic;

/// <summary>
/// Custom event module: Register/Unregister are no-ops.
/// Custom events are triggered manually via the API.
/// </summary>
public sealed class DynamicEventModule(CustomModuleDefinition def) : IEventModule
{
    public string ModuleId => def.Id;

    public ModuleManifest Manifest => new()
    {
        Id          = def.Id,
        Name        = def.Name,
        Description = def.Description,
        Category    = def.Category,
        Parameters  = def.Parameters
    };

    public void Register(string eventId, Dictionary<string, string> config,
                         Func<Dictionary<string, string>, Task> onFired)
    {
        // Custom events are triggered manually — no background listener needed.
    }

    public void Unregister(string eventId)
    {
        // Nothing to unregister.
    }
}
