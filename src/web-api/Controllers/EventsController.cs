using Microsoft.AspNetCore.Mvc;
using WorkflowEngine.Models;
using WorkflowEngine.Modules;
using WorkflowEngine.Services;

namespace WorkflowEngine.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(
    JsonDataService    data,
    WorkflowDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(data.GetAllEvents());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var evt = data.GetEvent(id);
        return evt is null ? NotFound() : Ok(evt);
    }

    [HttpPost]
    public IActionResult Create([FromBody] EventDefinition evt)
    {
        var created = data.AddEvent(evt);
        dispatcher.Register(created);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] EventDefinition evt)
    {
        var updated = data.UpdateEvent(id, evt);
        if (updated is null) return NotFound();
        dispatcher.Register(updated);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        dispatcher.Unregister(id);
        if (!data.DeleteEvent(id)) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/run")]
    public async Task<IActionResult> Run(string id)
    {
        var evt = data.GetEvent(id);
        if (evt is null) return NotFound();

        var context = new TriggerContext
        {
            EventId       = evt.Id,
            EventName     = evt.Name,
            EventModuleId = evt.ModuleId,
            Data          = []
        };

        await dispatcher.OnEventFired(context, evt);

        var runs = data.GetRuns();
        return Ok(runs.LastOrDefault());
    }

    [HttpPost("{id}/toggle")]
    public IActionResult Toggle(string id)
    {
        var evt = data.GetEvent(id);
        if (evt is null) return NotFound();

        evt.Enabled = !evt.Enabled;
        var updated = data.UpdateEvent(id, evt)!;
        dispatcher.Register(updated);
        return Ok(updated);
    }
}
