using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/workflows")]
public class WorkflowsController(JsonDataService data, ConditionEvaluator conditionEvaluator, ActionExecutor actionExecutor) : ControllerBase
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
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Workflow workflow)
    {
        var updated = data.UpdateWorkflow(id, workflow);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var deleted = data.DeleteWorkflow(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id}/run")]
    public async Task<IActionResult> Run(string id)
    {
        var workflow = data.GetWorkflow(id);
        if (workflow is null) return NotFound();

        var context = TriggerContext.Manual();
        var conditionsMet = conditionEvaluator.Evaluate(workflow.Conditions, context);

        var actionResults = new List<ActionResult>();
        var status = "success";

        if (conditionsMet)
        {
            foreach (var action in workflow.Actions)
            {
                var result = await actionExecutor.ExecuteAsync(action, context);
                actionResults.Add(result);
                if (result.Status == "failed" && !workflow.ContinueOnError)
                {
                    status = "failed";
                    break;
                }
                if (result.Status == "failed") status = "failed";
            }
        }

        var run = new Run
        {
            WorkflowId = id,
            TriggeredAt = DateTime.UtcNow,
            ConditionsMet = conditionsMet,
            ActionsExecuted = actionResults,
            Status = status
        };

        var saved = data.AddRun(run);
        return Ok(saved);
    }

    [HttpGet("{id}/runs")]
    public IActionResult GetRuns(string id) => Ok(data.GetRunsByWorkflow(id));
}
