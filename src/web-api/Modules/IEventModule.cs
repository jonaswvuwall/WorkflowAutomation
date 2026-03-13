namespace WorkflowEngine.Modules;

public interface IEventModule
{
    string ModuleId { get; }
    ModuleManifest Manifest { get; }

    /// <summary>
    /// Register an event. When it fires, call onFired with the assembled TriggerContext.
    /// eventId is the EventDefinition.Id.
    /// </summary>
    void Register(string eventId, string eventName,
                  Dictionary<string, string> config, Func<TriggerContext, Task> onFired);

    void Unregister(string eventId);
}
