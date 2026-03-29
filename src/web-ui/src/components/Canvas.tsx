import { useRef, useCallback } from 'react';
import ReactFlow, {
  Background, Controls, MiniMap,
  type Node, type Edge, type ReactFlowInstance,
} from 'reactflow';
import 'reactflow/dist/style.css';
import { EventNode } from './nodes/EventNode';
import { ActionNode } from './nodes/ActionNode';
import { ConditionNode } from './nodes/ConditionNode';
import type { WorkflowNodeData } from '../hooks/useCanvas';
import type { ModulesResponse } from '../types';

const nodeTypes = {
  event:     EventNode,
  action:    ActionNode,
  condition: ConditionNode,
};

interface CanvasProps {
  nodes:              Node<WorkflowNodeData>[];
  edges:              Edge[];
  onNodesChange:      Parameters<typeof ReactFlow>[0]['onNodesChange'];
  onEdgesChange:      Parameters<typeof ReactFlow>[0]['onEdgesChange'];
  onConnect:          Parameters<typeof ReactFlow>[0]['onConnect'];
  onNodeClick:        (node: Node<WorkflowNodeData>) => void;
  onNodeDoubleClick:  (node: Node<WorkflowNodeData>) => void;
  onDrop:             (e: React.DragEvent, rfInstance: ReactFlowInstance) => void;
  modules:            ModulesResponse;
}

export function Canvas({ nodes, edges, onNodesChange, onEdgesChange, onConnect, onNodeClick, onNodeDoubleClick, onDrop }: CanvasProps) {
  const rfRef = useRef<ReactFlowInstance | null>(null);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    if (rfRef.current) onDrop(e, rfRef.current);
  }, [onDrop]);

  return (
    <div className="canvas-wrapper" onDrop={handleDrop} onDragOver={handleDragOver}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        onNodeClick={(_, node) => onNodeClick(node as Node<WorkflowNodeData>)}
        onNodeDoubleClick={(_, node) => onNodeDoubleClick(node as Node<WorkflowNodeData>)}
        onInit={inst => { rfRef.current = inst; }}
        fitView
        deleteKeyCode="Delete"
      >
        <Background gap={16} />
        <Controls />
        <MiniMap />
      </ReactFlow>
    </div>
  );
}
