import { type Node, useReactFlow } from 'reactflow';
import type { WorkflowNodeData } from '../hooks/useCanvas';
import type { ModulesResponse, ParameterSchema } from '../types';

interface NodeInspectorProps {
  node:       Node<WorkflowNodeData> | null;
  modules:    ModulesResponse;
  onDeselect: () => void;
}

export function ParamField({ schema, value, onChange }: {
  schema:   ParameterSchema;
  value:    string;
  onChange: (val: string) => void;
}) {
  if (schema.type === 'textarea') {
    return (
      <div className="inspector__field">
        <label>{schema.label}{schema.required && ' *'}</label>
        <textarea value={value} onChange={e => onChange(e.target.value)} rows={3} />
      </div>
    );
  }
  if (schema.type === 'select') {
    return (
      <div className="inspector__field">
        <label>{schema.label}{schema.required && ' *'}</label>
        <select value={value} onChange={e => onChange(e.target.value)}>
          <option value="">— wählen —</option>
          {schema.options?.map(opt => (
            <option key={opt.value} value={opt.value}>{opt.label}</option>
          ))}
        </select>
      </div>
    );
  }
  return (
    <div className="inspector__field">
      <label>{schema.label}{schema.required && ' *'}</label>
      <input type="text" value={value} onChange={e => onChange(e.target.value)} />
    </div>
  );
}

export function NodeInspector({ node, modules, onDeselect }: NodeInspectorProps) {
  const { setNodes, setEdges } = useReactFlow();

  if (!node) {
    return (
      <aside className="inspector inspector--empty">
        <p>Klicke einen Node an, um ihn zu konfigurieren.</p>
      </aside>
    );
  }

  const allManifests = [...modules.events, ...modules.actions];
  const manifest = allManifests.find(m => m.id === node.data.moduleId);

  const updateConfig = (key: string, value: string) => {
    setNodes(nds => nds.map(n =>
      n.id === node.id
        ? { ...n, data: { ...n.data, config: { ...n.data.config, [key]: value } } }
        : n
    ));
  };

  const handleDelete = () => {
    setNodes(nds => nds.filter(n => n.id !== node.id));
    setEdges(eds => eds.filter(e => e.source !== node.id && e.target !== node.id));
    onDeselect();
  };

  return (
    <aside className="inspector">
      <h3 className="inspector__title">Node Config</h3>
      <div className="inspector__module-id">{node.data.moduleId}</div>

      {manifest && manifest.parameters.length > 0 ? (
        manifest.parameters
          .filter(p => !p.visibleWhen || node.data.config[p.visibleWhen.key] === p.visibleWhen.value)
          .map(param => (
          <ParamField
            key={param.key}
            schema={param}
            value={node.data.config[param.key] ?? ''}
            onChange={val => updateConfig(param.key, val)}
          />
        ))
      ) : (
        <p className="inspector__no-params">Dieses Modul hat keine Konfigurationsfelder.</p>
      )}

      <button className="btn btn--danger inspector__delete" onClick={handleDelete}>
        Node löschen
      </button>
    </aside>
  );
}
