using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Modules;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/modules")]
public class ModulesController(ModuleRegistry registry) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(registry.GetAllManifests());
}
