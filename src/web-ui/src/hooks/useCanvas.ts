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
 * Converts an event + its action chain into React Flow nodes/edges.
 * Event node → firstAction → nextAction → … (linked list)
 */
export function eventChainToFlow(
  event: EventDefinition,
  allActions: ActionDefinition[],
  modules: ModulesResponse
) {
  const nodes: Node<WorkflowNodeData>[] = [];
  const edges: Edge[] = [];

  nodes.push({
    id:       event.id,
    type:     'event',
    position: event.position,
    data: {
      label:    event.name || moduleLabel(event.moduleId, modules),
      moduleId: event.moduleId,
      config:   event.config,
      nodeType: 'event',
    },
  });

  let actionId = event.firstActionId;
  let prevId   = event.id;

  while (actionId) {
    const action = allActions.find(a => a.id === actionId);
    if (!action) break;

    nodes.push({
      id:       action.id,
      type:     'action',
      position: action.position,
      data: {
        label:    action.name || moduleLabel(action.moduleId, modules),
        moduleId: action.moduleId,
        config:   action.config,
        nodeType: 'action',
      },
    });

    edges.push({
      id:       `${prevId}->${action.id}`,
      source:   prevId,
      target:   action.id,
      animated: true,
    });

    prevId   = action.id;
    actionId = action.nextActionId;
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
