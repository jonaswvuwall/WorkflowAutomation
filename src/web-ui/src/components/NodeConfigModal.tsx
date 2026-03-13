import { useState } from 'react';
import { type Node, useReactFlow } from 'reactflow';
import { ParamField } from './NodeInspector';
import type { WorkflowNodeData } from '../hooks/useCanvas';
import type { ModulesResponse } from '../types';

interface NodeConfigModalProps {
  node:    Node<WorkflowNodeData>;
  modules: ModulesResponse;
  onClose: () => void;
}

export function NodeConfigModal({ node, modules, onClose }: NodeConfigModalProps) {
  const { setNodes } = useReactFlow();

  const allManifests = [...modules.events, ...modules.actions];
  const manifest = allManifests.find(m => m.id === node.data.moduleId);

  // Local copy of config for editing before save
  const [localConfig, setLocalConfig] = useState<Record<string, string>>({ ...node.data.config });

  const handleSave = () => {
    setNodes(nds => nds.map(n =>
      n.id === node.id
        ? { ...n, data: { ...n.data, config: localConfig } }
        : n
    ));
    onClose();
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={e => e.stopPropagation()}>
        <div className="modal__header">
          <h2 className="modal__title">{node.data.label}</h2>
          <div className="modal__module-id">{node.data.moduleId}</div>
        </div>

        <div className="modal__body">
          {manifest && manifest.parameters.length > 0 ? (
            manifest.parameters.map(param => (
              <ParamField
                key={param.key}
                schema={param}
                value={localConfig[param.key] ?? ''}
                onChange={val => setLocalConfig(cfg => ({ ...cfg, [param.key]: val }))}
              />
            ))
          ) : (
            <p className="inspector__no-params">Dieses Modul hat keine Konfigurationsfelder.</p>
          )}
        </div>

        <div className="modal__footer">
          <button className="btn" onClick={onClose}>Abbrechen</button>
          <button className="btn btn--primary" onClick={handleSave}>Speichern</button>
        </div>
      </div>
    </div>
  );
}
