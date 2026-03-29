using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/conditions")]
public class ConditionsController(JsonDataService data) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(data.GetAllConditions());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var cond = data.GetCondition(id);
        return cond is null ? NotFound() : Ok(cond);
    }

    [HttpPost]
    public IActionResult Create([FromBody] ConditionDefinition cond)
    {
        var created = data.AddCondition(cond);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] ConditionDefinition cond)
    {
        var updated = data.UpdateCondition(id, cond);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (!data.DeleteCondition(id)) return NotFound();
        return NoContent();
    }
}
