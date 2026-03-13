using WorkflowEngine.Models;

namespace WorkflowEngine.Modules;

public interface IRouterModule
{
    string ModuleId { get; }
    ModuleManifest Manifest { get; }

    /// <summary>
    /// Execute this router node: find all connected action nodes (via edges) and run them.
    /// </summary>
    Task<List<NodeExecutionResult>> ExecuteAsync(
        WorkflowNode routerNode,
        Workflow workflow,
        TriggerContext context,
        Func<string, IActionModule?> resolveAction);
}
