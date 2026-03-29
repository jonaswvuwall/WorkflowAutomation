using System.Text.Json;
using System.Text.Json.Nodes;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class JsonDataService
{
    private readonly string _dataDir;
    private readonly string _eventsFile;
    private readonly string _actionsFile;
    private readonly string _conditionsFile;
    private readonly string _logsFile;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonDataService(IWebHostEnvironment env)
    {
        _dataDir        = Path.Combine(env.ContentRootPath, "data");
        _eventsFile     = Path.Combine(_dataDir, "events.json");
        _actionsFile    = Path.Combine(_dataDir, "actions.json");
        _conditionsFile = Path.Combine(_dataDir, "conditions.json");
        _logsFile       = Path.Combine(_dataDir, "logs.json");
    }

    public void Initialize()
    {
        Directory.CreateDirectory(_dataDir);

        if (!File.Exists(_eventsFile))     File.WriteAllText(_eventsFile,     "[]");
        if (!File.Exists(_actionsFile))    File.WriteAllText(_actionsFile,    "[]");
        if (!File.Exists(_conditionsFile)) File.WriteAllText(_conditionsFile, "[]");

        if (!File.Exists(_logsFile))
            File.WriteAllText(_logsFile, "[]");
        else
            ClearLogsIfLegacy();

        MigrateEventsIfLegacy();
        MigrateActionsIfLegacy();
    }

    // ── Migration ─────────────────────────────────────────────────────────────

    private void ClearLogsIfLegacy()
    {
        try
        {
            var json = File.ReadAllText(_logsFile);
            var arr  = JsonNode.Parse(json) as JsonArray;
            if (arr is null || arr.Count == 0) return;
            if (arr[0]?["workflowId"] is not null || arr[0]?["nodeResults"] is not null)
                File.WriteAllText(_logsFile, "[]");
        }
        catch { File.WriteAllText(_logsFile, "[]"); }
    }

    /// <summary>Converts firstActionIds (string[]) → firstSteps (StepRef[]) in events.json.</summary>
    private void MigrateEventsIfLegacy()
    {
        try
        {
            var json = File.ReadAllText(_eventsFile);
            var arr  = JsonNode.Parse(json) as JsonArray;
            if (arr is null || arr.Count == 0 || arr[0]?["firstActionIds"] is null) return;

            foreach (var item in arr)
            {
                if (item is not JsonObject obj) continue;
                var ids        = obj["firstActionIds"]?.AsArray();
                var firstSteps = new JsonArray();
                if (ids is not null)
                    foreach (var id in ids)
                        firstSteps.Add(new JsonObject { ["id"] = id?.GetValue<string>(), ["type"] = "action" });
                obj.Remove("firstActionIds");
                obj["firstSteps"] = firstSteps;
            }
            File.WriteAllText(_eventsFile, arr.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* leave as-is; deserialization returns empty lists */ }
    }

    /// <summary>Converts nextActionIds (string[]) → nextSteps (StepRef[]) in actions.json.</summary>
    private void MigrateActionsIfLegacy()
    {
        try
        {
            var json = File.ReadAllText(_actionsFile);
            var arr  = JsonNode.Parse(json) as JsonArray;
            if (arr is null || arr.Count == 0 || arr[0]?["nextActionIds"] is null) return;

            foreach (var item in arr)
            {
                if (item is not JsonObject obj) continue;
                var ids       = obj["nextActionIds"]?.AsArray();
                var nextSteps = new JsonArray();
                if (ids is not null)
                    foreach (var id in ids)
                        nextSteps.Add(new JsonObject { ["id"] = id?.GetValue<string>(), ["type"] = "action" });
                obj.Remove("nextActionIds");
                obj["nextSteps"] = nextSteps;
            }
            File.WriteAllText(_actionsFile, arr.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* leave as-is */ }
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

    // ── Conditions ────────────────────────────────────────────────────────────

    public List<ConditionDefinition> GetAllConditions()
    {
        var json = File.ReadAllText(_conditionsFile);
        return JsonSerializer.Deserialize<List<ConditionDefinition>>(json, _jsonOptions) ?? [];
    }

    public ConditionDefinition? GetCondition(string id)
        => GetAllConditions().FirstOrDefault(c => c.Id == id);

    public ConditionDefinition AddCondition(ConditionDefinition cond)
    {
        cond.Id = Guid.NewGuid().ToString("N")[..8];
        var all = GetAllConditions();
        all.Add(cond);
        SaveConditions(all);
        return cond;
    }

    public ConditionDefinition? UpdateCondition(string id, ConditionDefinition cond)
    {
        var all   = GetAllConditions();
        var index = all.FindIndex(c => c.Id == id);
        if (index < 0) return null;
        cond.Id    = id;
        all[index] = cond;
        SaveConditions(all);
        return cond;
    }

    public bool DeleteCondition(string id)
    {
        var all     = GetAllConditions();
        var removed = all.RemoveAll(c => c.Id == id);
        if (removed == 0) return false;
        SaveConditions(all);
        return true;
    }

    private void SaveConditions(List<ConditionDefinition> conditions)
        => File.WriteAllText(_conditionsFile, JsonSerializer.Serialize(conditions, _jsonOptions));

    // ── Runs ──────────────────────────────────────────────────────────────────

    public List<Run> GetRuns()
    {
        var json = File.ReadAllText(_logsFile);
        return JsonSerializer.Deserialize<List<Run>>(json, _jsonOptions) ?? [];
    }

    public Run? GetRun(string id)
        => GetRuns().FirstOrDefault(r => r.Id == id);

    public Run AddRun(Run run)
    {
        run.Id = Guid.NewGuid().ToString();
        var runs = GetRuns();
        runs.Add(run);
        File.WriteAllText(_logsFile, JsonSerializer.Serialize(runs, _jsonOptions));
        return run;
    }
}
