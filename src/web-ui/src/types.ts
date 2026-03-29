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
  events:     ModuleManifest[];
  conditions: ModuleManifest[];
  actions:    ModuleManifest[];
}

// ── Data model ─────────────────────────────────────────────────────────────

export interface NodePosition {
  x: number;
  y: number;
}

export interface NodeUi {
  position: NodePosition;
}

export interface StepRef {
  id:   string;
  type: 'action' | 'condition';
}

export interface EventDefinition {
  id:         string;
  name:       string;
  enabled:    boolean;
  moduleId:   string;
  config:     Record<string, string>;
  firstSteps: StepRef[];
  ui:         NodeUi;
}

export interface ActionDefinition {
  id:        string;
  name:      string;
  moduleId:  string;
  config:    Record<string, string>;
  nextSteps: StepRef[];
  ui:        NodeUi;
}

export interface ConditionDefinition {
  id:             string;
  name:           string;
  moduleId:       string;
  config:         Record<string, string>;
  trueNextSteps:  StepRef[];
  falseNextSteps: StepRef[];
  ui:             NodeUi;
}

// ── Runs ──────────────────────────────────────────────────────────────────

export interface ActionExecutionResult {
  actionId:  string;
  moduleId:  string;
  status:    'success' | 'failed';
  message?:  string;
}

export interface ConditionStepResult {
  conditionId: string;
  moduleId:    string;
  result:      boolean;
  message:     string;
}

export interface Run {
  id:               string;
  eventId:          string;
  eventName:        string;
  triggeredAt:      string;
  status:           'success' | 'failed' | 'pending';
  actionResults:    ActionExecutionResult[];
  conditionResults: ConditionStepResult[];
  error?:           string;
}

// ── Status ────────────────────────────────────────────────────────────────

export interface StatusData {
  status:    string;
  startedAt: string;
  events:    { total: number; enabled: number };
  runs:      { total: number; success: number; failed: number };
}
