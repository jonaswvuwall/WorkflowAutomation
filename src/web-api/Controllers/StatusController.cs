using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController(JsonDataService data) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var workflows = data.GetWorkflows();
        var runs = data.GetRuns();

        return Ok(new
        {
            status = "running",
            startedAt = DateTime.UtcNow,
            workflows = new
            {
                total = workflows.Count,
                enabled = workflows.Count(w => w.Enabled)
            },
            runs = new
            {
                total = runs.Count,
                success = runs.Count(r => r.Status == "success"),
                failed = runs.Count(r => r.Status == "failed")
            }
        });
    }
}
