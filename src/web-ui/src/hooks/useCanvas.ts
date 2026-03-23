import { useCallback } from 'react';
import {
  useNodesState, useEdgesState, addEdge,
  type Node, type Edge, type Connection, type OnConnect,
} from 'reactflow';
import type { EventDefinition, ActionDefinition, ModulesResponse } from '../types';

// Data stored inside each React Flow node
export interface WorkflowNodeData {
  label:    string;
  moduleId: string;
  config:   Record<string, string>;
  nodeType: 'event' | 'action';
}

function moduleLabel(moduleId: string, modules: ModulesResponse): string {
  const all = [...modules.events, ...modules.actions];
  return all.find(m => m.id === moduleId)?.name ?? moduleId;
}

/**
 * Converts an event + its action graph into React Flow nodes/edges.
 * Supports multiple outgoing edges per node (parallel branches).
 */
export function eventChainToFlow(
  event: EventDefinition,
  allActions: ActionDefinition[],
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

  const queue: Array<{ actionId: string; sourceId: string }> =
    event.firstActionIds.map(id => ({ actionId: id, sourceId: event.id }));

  while (queue.length > 0) {
    const { actionId, sourceId } = queue.shift()!;
    const action = allActions.find(a => a.id === actionId);
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
      id:       `${sourceId}->${action.id}`,
      source:   sourceId,
      target:   action.id,
      animated: true,
    });

    for (const nextId of action.nextActionIds) {
      queue.push({ actionId: nextId, sourceId: action.id });
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
