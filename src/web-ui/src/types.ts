// ── Module manifests (from GET /api/modules) ──────────────────────────────

export interface SelectOption {
  value: string;
  label: string;
}

export interface VisibleWhen {
  key:   string;
  value: string;
}

export interface ParameterSchema {
  key:          string;
  label:        string;
  type:         'text' | 'textarea' | 'select' | 'number' | 'toggle';
  required:     boolean;
  default?:     string;
  options?:     SelectOption[];
  visibleWhen?: VisibleWhen;
}

export interface ModuleManifest {
  id:          string;
  name:        string;
  description: string;
  category:    string;
  parameters:  ParameterSchema[];
}

export interface ModulesResponse {
  events:  ModuleManifest[];
  actions: ModuleManifest[];
}

// ── Data model ─────────────────────────────────────────────────────────────

export interface NodePosition {
  x: number;
  y: number;
}

export interface NodeUi {
  position: NodePosition;
}

export interface EventDefinition {
  id:            string;
  name:          string;
  enabled:       boolean;
  moduleId:      string;
  config:        Record<string, string>;
  firstActionIds: string[];
  ui:            NodeUi;
}

export interface ActionDefinition {
  id:           string;
  name:         string;
  moduleId:     string;
  config:       Record<string, string>;
  nextActionIds: string[];
  ui:           NodeUi;
}

// ── Runs ──────────────────────────────────────────────────────────────────

export interface ActionExecutionResult {
  actionId:  string;
  moduleId:  string;
  status:    'success' | 'failed';
  message?:  string;
}

export interface Run {
  id:            string;
  eventId:       string;
  eventName:     string;
  triggeredAt:   string;
  status:        'success' | 'failed' | 'pending';
  actionResults: ActionExecutionResult[];
  error?:        string;
}

// ── Status ────────────────────────────────────────────────────────────────

export interface StatusData {
  status:    string;
  startedAt: string;
  events:    { total: number; enabled: number };
  runs:      { total: number; success: number; failed: number };
}
