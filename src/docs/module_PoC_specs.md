# Module Engine – Lokale JSON

## Konzept

- Keine Datenbank – alle Daten werden in lokalen JSON-Dateien gespeichert
- Dateien liegen im Projektordner unter `/workflow/result`

## Simple API Befehle

- `GET /api/workflows` – Alle Workflows anzeigen
- `GET /api/workflows/{id}` – Einzelnen Workflow laden
- `POST /api/workflows` – Neuen Workflow erstellen
- `PUT /api/workflows/{id}` – Workflow aktualisieren
- `DELETE /api/workflows/{id}` – Workflow löschen
- `POST /api/workflows/{id}/run` – Workflow manuell starten
- `GET /api/runs` – Alle Workflow-Ausführungen anzeigen
- `GET /api/runs/{id}` – Einzelnen Run anzeigen
- `GET /api/workflows/{id}/runs` – Runs eines Workflows anzeigen
- `GET /api/status` – Status der Engine anzeigen

## Workflow Ablauf

Event triggers first action
Action triggers next action and so on

## JSON Struktur

### events.json

```json
[
  {
    "id":            "a1b2c3d4",
    "name":          "Watch C:\\temp for new files",
    "enabled":       true,
    "moduleId":      "event.file_watcher",
    "config": {
      "path":  "C:\\temp",
      "event": "created"
    },
    "firstActionId": "e5f6a7b8",
    "ui": {
      "position": { "x": 0, "y": 0 }
    }
  }
]
```

### actions.json

```json
[
  {
    "id":           "e5f6a7b8",
    "name":         "Log detected file",
    "moduleId":     "action.log",
    "config": {
      "message": "File detected: {{filePath}}"
    },
    "nextActionId": "k7l8m9n0",
    "ui": {
      "position": { "x": 250, "y": 0 }
    }
  },
  {
    "id":           "k7l8m9n0",
    "name":         "Copy to backup",
    "moduleId":     "action.copy_file",
    "config": {
      "source":      "{{filePath}}",
      "destination": "C:\\backup\\{{fileName}}"
    },
    "nextActionId": null,
    "ui": {
      "position": { "x": 500, "y": 0 }
    }
  }
]
```

### logs.json

```json
[
  {
    "id":          "550e8400-e29b-41d4-a716-446655440000",
    "eventId":     "a1b2c3d4",
    "eventName":   "Watch C:\\temp for new files",
    "triggeredAt": "2026-03-13T10:42:00Z",
    "status":      "success",
    "actionResults": [
      {
        "actionId": "e5f6a7b8",
        "moduleId": "action.log",
        "status":   "success",
        "message":  "File detected: C:\\temp\\bericht.txt"
      },
      {
        "actionId": "k7l8m9n0",
        "moduleId": "action.copy_file",
        "status":   "success",
        "message":  "Copied C:\\temp\\bericht.txt → C:\\backup\\bericht.txt"
      }
    ]
  }
]
```
