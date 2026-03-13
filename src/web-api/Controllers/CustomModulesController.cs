using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Models;
using WorkflowEngine.Modules;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/custom-modules")]
public class CustomModulesController(
    CustomModulesService service,
    DynamicModuleLoader loader) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<CustomModuleDefinition>> GetAll()
        => service.GetAll();

    [HttpPost]
    public ActionResult<CustomModuleDefinition> Create([FromBody] CustomModuleDefinition def)
    {
        var created = service.Add(def);
        loader.Load(created);
        return CreatedAtAction(nameof(GetAll), created);
    }

    [HttpPut("{id}")]
    public ActionResult<CustomModuleDefinition> Update(string id, [FromBody] CustomModuleDefinition def)
    {
        var updated = service.Update(id, def);
        if (updated is null) return NotFound();
        loader.Unload(id);
        loader.Load(updated);
        return updated;
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (!service.Delete(id)) return NotFound();
        loader.Unload(id);
        return NoContent();
    }
}
