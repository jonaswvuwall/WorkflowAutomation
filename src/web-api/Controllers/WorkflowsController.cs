using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/workflows")]
public class WorkflowsController(JsonDataService data, ActionExecutor actionExecutor, FileWatcherService watcher) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(data.GetWorkflows());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var workflow = data.GetWorkflow(id);
        return workflow is null ? NotFound() : Ok(workflow);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Workflow workflow)
    {
        var created = data.AddWorkflow(workflow);
        watcher.Register(created);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Workflow workflow)
    {
        var updated = data.UpdateWorkflow(id, workflow);
        if (updated is null) return NotFound();
        watcher.Register(updated);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var deleted = data.DeleteWorkflow(id);
        if (!deleted) return NotFound();
        watcher.Unregister(id);
        return NoContent();
    }

    [HttpPost("{id}/run")]
    public async Task<IActionResult> Run(string id)
    {
        var workflow = data.GetWorkflow(id);
        if (workflow is null) return NotFound();

        var context = TriggerContext.Manual();
        var result = await actionExecutor.ExecuteAsync(workflow.Then, context);

        var run = new Run
        {
            WorkflowId = id,
            TriggeredAt = DateTime.UtcNow,
            ActionExecuted = result,
            Status = result.Status
        };

        return Ok(data.AddRun(run));
    }

    [HttpGet("{id}/runs")]
    public IActionResult GetRuns(string id) => Ok(data.GetRunsByWorkflow(id));
}
