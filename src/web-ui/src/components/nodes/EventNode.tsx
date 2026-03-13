import { Handle, Position, type NodeProps } from 'reactflow';
import type { WorkflowNodeData } from '../../hooks/useCanvas';

export function EventNode({ data, selected }: NodeProps<WorkflowNodeData>) {
  return (
    <div className={`wf-node wf-node--event ${selected ? 'wf-node--selected' : ''}`}>
      <div className="wf-node__header">
        <span className="wf-node__badge">EVENT</span>
        <span className="wf-node__name">{data.label}</span>
      </div>
      {data.moduleId && (
        <div className="wf-node__module">{data.moduleId}</div>
      )}
      <Handle type="source" position={Position.Right} />
    </div>
  );
}
