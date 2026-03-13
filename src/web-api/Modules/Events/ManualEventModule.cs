namespace WorkflowEngine.Modules.Events;

public sealed class ManualEventModule : IEventModule
{
    public string ModuleId => "event.manual";

    public ModuleManifest Manifest => new()
    {
        Id          = "event.manual",
        Name        = "Manual Trigger",
        Description = "Triggered manually via the API",
        Category    = "General",
        Parameters  = []
    };

    public void Register(string eventId, string eventName,
                         Dictionary<string, string> config, Func<TriggerContext, Task> onFired)
    {
        // No background listener needed — manual triggers are fired via API
    }

    public void Unregister(string eventId) { }
}
