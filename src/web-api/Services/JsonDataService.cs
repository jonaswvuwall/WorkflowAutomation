using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class JsonDataService
{
    private readonly string _dataDir;
    private readonly string _eventsFile;
    private readonly string _actionsFile;
    private readonly string _runsFile;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonDataService(IWebHostEnvironment env)
    {
        _dataDir     = Path.Combine(env.ContentRootPath, "data");
        _eventsFile  = Path.Combine(_dataDir, "events.json");
        _actionsFile = Path.Combine(_dataDir, "actions.json");
        _runsFile    = Path.Combine(_dataDir, "runs.json");
    }

    public void Initialize()
    {
        Directory.CreateDirectory(_dataDir);

        if (!File.Exists(_eventsFile))  File.WriteAllText(_eventsFile,  "[]");
        if (!File.Exists(_actionsFile)) File.WriteAllText(_actionsFile, "[]");

        if (!File.Exists(_runsFile))
            File.WriteAllText(_runsFile, "[]");
        else
            ClearRunsIfLegacy();
    }

    private void ClearRunsIfLegacy()
    {
        try
        {
            var json = File.ReadAllText(_runsFile);
            var arr  = System.Text.Json.Nodes.JsonNode.Parse(json) as System.Text.Json.Nodes.JsonArray;
            if (arr is null || arr.Count == 0) return;
            // Old format had "workflowId" or "nodeResults" — clear it
            if (arr[0]?["workflowId"] is not null || arr[0]?["nodeResults"] is not null)
                File.WriteAllText(_runsFile, "[]");
        }
        catch { File.WriteAllText(_runsFile, "[]"); }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    public List<EventDefinition> GetAllEvents()
    {
        var json = File.ReadAllText(_eventsFile);
        return JsonSerializer.Deserialize<List<EventDefinition>>(json, _jsonOptions) ?? [];
    }

    public EventDefinition? GetEvent(string id)
        => GetAllEvents().FirstOrDefault(e => e.Id == id);

    public EventDefinition AddEvent(EventDefinition evt)
    {
        evt.Id = Guid.NewGuid().ToString("N")[..8];
        var all = GetAllEvents();
        all.Add(evt);
        SaveEvents(all);
        return evt;
    }

    public EventDefinition? UpdateEvent(string id, EventDefinition evt)
    {
        var all   = GetAllEvents();
        var index = all.FindIndex(e => e.Id == id);
        if (index < 0) return null;
        evt.Id     = id;
        all[index] = evt;
        SaveEvents(all);
        return evt;
    }

    public bool DeleteEvent(string id)
    {
        var all     = GetAllEvents();
        var removed = all.RemoveAll(e => e.Id == id);
        if (removed == 0) return false;
        SaveEvents(all);
        return true;
    }

    private void SaveEvents(List<EventDefinition> events)
        => File.WriteAllText(_eventsFile, JsonSerializer.Serialize(events, _jsonOptions));

    // ── Actions ───────────────────────────────────────────────────────────────

    public List<ActionDefinition> GetAllActions()
    {
        var json = File.ReadAllText(_actionsFile);
        return JsonSerializer.Deserialize<List<ActionDefinition>>(json, _jsonOptions) ?? [];
    }

    public ActionDefinition? GetAction(string id)
        => GetAllActions().FirstOrDefault(a => a.Id == id);

    public ActionDefinition AddAction(ActionDefinition action)
    {
        action.Id = Guid.NewGuid().ToString("N")[..8];
        var all = GetAllActions();
        all.Add(action);
        SaveActions(all);
        return action;
    }

    public ActionDefinition? UpdateAction(string id, ActionDefinition action)
    {
        var all   = GetAllActions();
        var index = all.FindIndex(a => a.Id == id);
        if (index < 0) return null;
        action.Id  = id;
        all[index] = action;
        SaveActions(all);
        return action;
    }

    public bool DeleteAction(string id)
    {
        var all     = GetAllActions();
        var removed = all.RemoveAll(a => a.Id == id);
        if (removed == 0) return false;
        SaveActions(all);
        return true;
    }

    private void SaveActions(List<ActionDefinition> actions)
        => File.WriteAllText(_actionsFile, JsonSerializer.Serialize(actions, _jsonOptions));

    // ── Runs ──────────────────────────────────────────────────────────────────

    public List<Run> GetRuns()
    {
        var json = File.ReadAllText(_runsFile);
        return JsonSerializer.Deserialize<List<Run>>(json, _jsonOptions) ?? [];
    }

    public Run? GetRun(string id)
        => GetRuns().FirstOrDefault(r => r.Id == id);

    public Run AddRun(Run run)
    {
        run.Id = Guid.NewGuid().ToString();
        var runs = GetRuns();
        runs.Add(run);
        File.WriteAllText(_runsFile, JsonSerializer.Serialize(runs, _jsonOptions));
        return run;
    }
}
