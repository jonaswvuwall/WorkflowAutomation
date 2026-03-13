namespace WorkflowEngine.Modules;

public interface IEventModule
{
    string ModuleId { get; }
    ModuleManifest Manifest { get; }

    /// <summary>
    /// Start listening for this event. Call onFired with any extra data when the event occurs
    /// (e.g. { "filePath": "..." }). The dispatcher builds the full TriggerContext.
    /// </summary>
    void Register(string eventId, Dictionary<string, string> config,
                  Func<Dictionary<string, string>, Task> onFired);

    void Unregister(string eventId);
}
