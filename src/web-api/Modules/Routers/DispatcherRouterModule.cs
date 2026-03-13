using WorkflowEngine.Models;

namespace WorkflowEngine.Modules.Routers;

/// <summary>
/// Default router module: finds all action nodes connected via outgoing edges
/// and executes them sequentially (fail-fast on error).
/// </summary>
public sealed class DispatcherRouterModule(ILogger<DispatcherRouterModule> logger)
    : IRouterModule
{
    public string ModuleId => "router.dispatcher";

    public ModuleManifest Manifest => new()
    {
        Id          = "router.dispatcher",
        Name        = "Dispatcher",
        Description = "Executes all connected action modules sequentially",
        Category    = "General",
        Parameters  = []
    };

    public async Task<List<NodeExecutionResult>> ExecuteAsync(
        WorkflowNode routerNode,
        Workflow workflow,
        TriggerContext context,
        Func<string, IActionModule?> resolveAction)
    {
        // Find all action nodes connected via outgoing edges from this router node
        var targetNodeIds = workflow.Edges
            .Where(e => e.Source == routerNode.Id)
            .Select(e => e.Target)
            .ToHashSet();

        var actionNodes = workflow.Nodes
            .Where(n => targetNodeIds.Contains(n.Id) && n.Type == "action")
            .ToList();

        var results = new List<NodeExecutionResult>();

        foreach (var actionNode in actionNodes)
        {
            var module = resolveAction(actionNode.ModuleId);
            if (module is null)
            {
                logger.LogWarning("No action module found for '{ModuleId}' (node {NodeId})",
                    actionNode.ModuleId, actionNode.Id);
                results.Add(new NodeExecutionResult
                {
                    NodeId   = actionNode.Id,
                    ModuleId = actionNode.ModuleId,
                    Status   = "failed",
                    Message  = $"Unknown action module '{actionNode.ModuleId}'"
                });
                break;
            }

            try
            {
                var result = await module.ExecuteAsync(actionNode.Id, actionNode.Config, context);
                results.Add(result);
                if (result.Status == "failed")
                    break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Action module {ModuleId} threw on node {NodeId}",
                    actionNode.ModuleId, actionNode.Id);
                results.Add(new NodeExecutionResult
                {
                    NodeId   = actionNode.Id,
                    ModuleId = actionNode.ModuleId,
                    Status   = "failed",
                    Message  = ex.Message
                });
                break;
            }
        }

        return results;
    }
}
