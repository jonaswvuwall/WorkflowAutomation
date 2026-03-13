// ── Module manifests (from GET /api/modules) ──────────────────────────────

export interface SelectOption {
  value: string;
  label: string;
}

export interface ParameterSchema {
  key:      string;
  label:    string;
  type:     'text' | 'textarea' | 'select' | 'number' | 'toggle';
  required: boolean;
  options?: SelectOption[];
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

// ── Custom module definitions ──────────────────────────────────────────────

export interface CustomModuleDefinition {
  id:             string;
  name:           string;
  description:    string;
  category:       string;
  moduleType:     'event' | 'action';
  baseType:       'script' | 'http_request';
  scriptContent?: string;
  httpMethod?:    string;
  httpUrl?:       string;
  httpBody?:      string;
  parameters:     ParameterSchema[];
}

// ── Data model ─────────────────────────────────────────────────────────────

export interface NodePosition {
  x: number;
  y: number;
}

export interface EventDefinition {
  id:            string;
  name:          string;
  enabled:       boolean;
  moduleId:      string;
  config:        Record<string, string>;
  firstActionId: string | null;
  position:      NodePosition;
}

export interface ActionDefinition {
  id:           string;
  name:         string;
  moduleId:     string;
  config:       Record<string, string>;
  nextActionId: string | null;
  position:     NodePosition;
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
