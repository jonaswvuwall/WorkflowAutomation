import { Handle, Position, type NodeProps } from 'reactflow';
import type { WorkflowNodeData } from '../../hooks/useCanvas';

export function ExecutionNode({ data, selected }: NodeProps<WorkflowNodeData>) {
  return (
    <div className={`wf-node wf-node--execution ${selected ? 'wf-node--selected' : ''}`}>
      <Handle type="target" position={Position.Left} />
      <div className="wf-node__header">
        <span className="wf-node__badge">EXEC</span>
        <span className="wf-node__name">{data.label}</span>
      </div>
      {data.moduleId && (
        <div className="wf-node__module">{data.moduleId}</div>
      )}
      <Handle type="source" position={Position.Right} />
    </div>
  );
}
