import { Handle, Position, type NodeProps } from 'reactflow';
import type { WorkflowNodeData } from '../../hooks/useCanvas';

export function ConditionNode({ data, selected }: NodeProps<WorkflowNodeData>) {
  return (
    <div className={`wf-node wf-node--condition ${selected ? 'wf-node--selected' : ''}`}>
      <Handle type="target" position={Position.Left} />
      <div className="wf-node__header">
        <span className="wf-node__badge">IF</span>
        <span className="wf-node__name">{data.label}</span>
      </div>
      {data.moduleId && (
        <div className="wf-node__module">{data.moduleId}</div>
      )}
      <div className="wf-node__branches">
        <div className="wf-node__branch wf-node__branch--true">
          <span>Yes</span>
          <Handle type="source" position={Position.Right} id="true" className="wf-node__branch-handle" />
        </div>
        <div className="wf-node__branch wf-node__branch--false">
          <span>No</span>
          <Handle type="source" position={Position.Right} id="false" className="wf-node__branch-handle" />
        </div>
      </div>
    </div>
  );
}
