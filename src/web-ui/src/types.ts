export interface WorkflowTrigger {
  type: string;
  path?: string | null;
}

export interface ActionResult {
  type: string;
  status: 'success' | 'failed';
  message?: string;
}

export interface Condition {
  field: string;
  operator: string;
  value: string;
}

export interface WorkflowAction {
  type: string;
  parameters: Record<string, string>;
}

export interface Workflow {
  id: string;
  name: string;
  enabled: boolean;
  trigger: WorkflowTrigger;
  continueOnError: boolean;
  conditions: Condition[];
  actions: WorkflowAction[];
  createdAt: string;
}

export interface Run {
  workflowId: string;
  triggeredAt: string;
  status: 'success' | 'failed';
  conditionsMet: boolean;
  actionsExecuted: ActionResult[];
  error?: string;
}

export interface StatusData {
  status: string;
  workflows: { enabled: number; total: number };
  runs: { total: number; success: number; failed: number };
}
