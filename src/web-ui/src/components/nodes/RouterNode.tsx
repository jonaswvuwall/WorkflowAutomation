import { Handle, Position, type NodeProps } from 'reactflow';
import type { WorkflowNodeData } from '../../hooks/useCanvas';

export function RouterNode({ data, selected }: NodeProps<WorkflowNodeData>) {
  return (
    <div className={`wf-node wf-node--router ${selected ? 'wf-node--selected' : ''}`}>
      <Handle type="target" position={Position.Left} />
      <div className="wf-node__header">
        <span className="wf-node__badge">ROUTER</span>
        <span className="wf-node__name">{data.label}</span>
      </div>
      {data.moduleId && (
        <div className="wf-node__module">{data.moduleId}</div>
      )}
      <Handle type="source" position={Position.Right} />
    </div>
  );
}
