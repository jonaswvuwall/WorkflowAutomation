using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Models;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/actions")]
public class ActionsController(JsonDataService data) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(data.GetAllActions());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var action = data.GetAction(id);
        return action is null ? NotFound() : Ok(action);
    }

    [HttpPost]
    public IActionResult Create([FromBody] ActionDefinition action)
    {
        var created = data.AddAction(action);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] ActionDefinition action)
    {
        var updated = data.UpdateAction(id, action);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (!data.DeleteAction(id)) return NotFound();
        return NoContent();
    }
}
