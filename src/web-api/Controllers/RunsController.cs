using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/runs")]
public class RunsController(JsonDataService data) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(data.GetRuns());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var run = data.GetRun(id);
        return run is null ? NotFound() : Ok(run);
    }
}
