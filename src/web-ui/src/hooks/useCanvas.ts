import { useCallback } from 'react';
import {
  useNodesState, useEdgesState, addEdge,
  type Node, type Edge, type Connection, type OnConnect,
} from 'reactflow';
import type { EventDefinition, ActionDefinition, ConditionDefinition, ModulesResponse, StepRef } from '../types';

// Data stored inside each React Flow node
export interface WorkflowNodeData {
  label:    string;
  moduleId: string;
  config:   Record<string, string>;
  nodeType: 'event' | 'action' | 'condition';
}

function moduleLabel(moduleId: string, modules: ModulesResponse): string {
  const all = [...modules.events, ...modules.conditions, ...modules.actions];
  return all.find(m => m.id === moduleId)?.name ?? moduleId;
}

/**
 * Converts an event + its step graph into React Flow nodes/edges.
 * Supports actions, conditions (with true/false branches), and parallel fan-out.
 */
export function eventChainToFlow(
  event: EventDefinition,
  allActions: ActionDefinition[],
  allConditions: ConditionDefinition[],
  modules: ModulesResponse
) {
  const nodes: Node<WorkflowNodeData>[] = [];
  const edges: Edge[] = [];
  const visited = new Set<string>();

  nodes.push({
    id:       event.id,
    type:     'event',
    position: event.ui.position,
    data: {
      label:    event.name || moduleLabel(event.moduleId, modules),
      moduleId: event.moduleId,
      config:   event.config,
      nodeType: 'event',
    },
  });

  type QueueItem = { step: StepRef; sourceId: string; sourceHandle?: string };

  const queue: QueueItem[] = event.firstSteps.map(step => ({ step, sourceId: event.id }));

  while (queue.length > 0) {
    const { step, sourceId, sourceHandle } = queue.shift()!;

    if (step.type === 'action') {
      const action = allActions.find(a => a.id === step.id);
      if (!action) continue;

      if (!visited.has(action.id)) {
        visited.add(action.id);
        nodes.push({
          id:       action.id,
          type:     'action',
          position: action.ui.position,
          data: {
            label:    action.name || moduleLabel(action.moduleId, modules),
            moduleId: action.moduleId,
            config:   action.config,
            nodeType: 'action',
          },
        });
      }

      edges.push({
        id:           `${sourceId}${sourceHandle ?? ''}->${action.id}`,
        source:       sourceId,
        target:       action.id,
        sourceHandle: sourceHandle ?? null,
        animated:     true,
      });

      for (const next of action.nextSteps) {
        queue.push({ step: next, sourceId: action.id });
      }

    } else if (step.type === 'condition') {
      const cond = allConditions.find(c => c.id === step.id);
      if (!cond) continue;

      if (!visited.has(cond.id)) {
        visited.add(cond.id);
        nodes.push({
          id:       cond.id,
          type:     'condition',
          position: cond.ui.position,
          data: {
            label:    cond.name || moduleLabel(cond.moduleId, modules),
            moduleId: cond.moduleId,
            config:   cond.config,
            nodeType: 'condition',
          },
        });
      }

      edges.push({
        id:           `${sourceId}${sourceHandle ?? ''}->${cond.id}`,
        source:       sourceId,
        target:       cond.id,
        sourceHandle: sourceHandle ?? null,
        animated:     true,
      });

      for (const next of cond.trueNextSteps) {
        queue.push({ step: next, sourceId: cond.id, sourceHandle: 'true' });
      }
      for (const next of cond.falseNextSteps) {
        queue.push({ step: next, sourceId: cond.id, sourceHandle: 'false' });
      }
    }
  }

  return { nodes, edges };
}


export function useCanvas(initialNodes: Node<WorkflowNodeData>[] = [], initialEdges: Edge[] = []) {
  const [nodes, setNodes, onNodesChange] = useNodesState<WorkflowNodeData>(initialNodes);
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);

  const onConnect: OnConnect = useCallback(
    (connection: Connection) => setEdges(eds => addEdge({ ...connection, animated: true }, eds)),
    [setEdges]
  );

  return { nodes, setNodes, onNodesChange, edges, setEdges, onEdgesChange, onConnect };
}
