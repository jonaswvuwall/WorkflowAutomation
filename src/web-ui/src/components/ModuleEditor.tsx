import { useState } from 'react';
import type { CustomModuleDefinition, ParameterSchema } from '../types';

const EMPTY_DEF: Omit<CustomModuleDefinition, 'id'> = {
  name:          '',
  description:   '',
  category:      'Custom',
  moduleType:    'action',
  baseType:      'script',
  scriptContent: '',
  httpMethod:    'POST',
  httpUrl:       '',
  httpBody:      '{}',
  parameters:    [],
};

interface ModuleEditorProps {
  initial?: CustomModuleDefinition;
  onSave:   (def: Omit<CustomModuleDefinition, 'id'>) => void;
  onCancel: () => void;
}

export function ModuleEditor({ initial, onSave, onCancel }: ModuleEditorProps) {
  const [def, setDef] = useState<Omit<CustomModuleDefinition, 'id'>>(
    initial ? (({ id: _id, ...rest }) => rest)(initial) : EMPTY_DEF
  );

  const set = <K extends keyof typeof def>(key: K, value: (typeof def)[K]) =>
    setDef(d => ({ ...d, [key]: value }));

  const addParam = () =>
    set('parameters', [...def.parameters, { key: '', label: '', type: 'text', required: false }]);

  const updateParam = (i: number, patch: Partial<ParameterSchema>) =>
    set('parameters', def.parameters.map((p, idx) => idx === i ? { ...p, ...patch } : p));

  const removeParam = (i: number) =>
    set('parameters', def.parameters.filter((_, idx) => idx !== i));

  return (
    <div className="module-editor">
      <div className="module-editor__grid">
        <div className="inspector__field">
          <label>Name *</label>
          <input value={def.name} onChange={e => set('name', e.target.value)} />
        </div>
        <div className="inspector__field">
          <label>Beschreibung</label>
          <input value={def.description} onChange={e => set('description', e.target.value)} />
        </div>
        <div className="inspector__field">
          <label>Kategorie</label>
          <input value={def.category} onChange={e => set('category', e.target.value)} />
        </div>
        <div className="inspector__field">
          <label>Modul-Typ</label>
          <select value={def.moduleType} onChange={e => set('moduleType', e.target.value as CustomModuleDefinition['moduleType'])}>
            <option value="event">Event</option>
            <option value="action">Action</option>
          </select>
        </div>
        <div className="inspector__field">
          <label>Base Type</label>
          <select value={def.baseType} onChange={e => set('baseType', e.target.value as CustomModuleDefinition['baseType'])}>
            <option value="script">PowerShell Script</option>
            <option value="http_request">HTTP Request</option>
          </select>
        </div>
      </div>

      {def.baseType === 'script' && (
        <div className="module-editor__section">
          <h4 className="module-editor__section-title">Script</h4>
          <p className="module-editor__hint">{'{{paramKey}} wird durch den Parameterwert ersetzt'}</p>
          <div className="inspector__field">
            <textarea
              value={def.scriptContent ?? ''}
              onChange={e => set('scriptContent', e.target.value)}
              rows={8}
              className="module-editor__code"
              placeholder="Write-Host 'Hello {{name}}'"
            />
          </div>
        </div>
      )}

      {def.baseType === 'http_request' && (
        <div className="module-editor__section">
          <h4 className="module-editor__section-title">HTTP Request</h4>
          <div className="module-editor__http-row">
            <div className="inspector__field module-editor__method">
              <label>Method</label>
              <select value={def.httpMethod ?? 'POST'} onChange={e => set('httpMethod', e.target.value)}>
                <option>GET</option>
                <option>POST</option>
                <option>PUT</option>
                <option>PATCH</option>
                <option>DELETE</option>
              </select>
            </div>
            <div className="inspector__field module-editor__url">
              <label>URL</label>
              <input value={def.httpUrl ?? ''} onChange={e => set('httpUrl', e.target.value)} placeholder="https://example.com/{{endpoint}}" />
            </div>
          </div>
          <div className="inspector__field">
            <label>Body (JSON)</label>
            <textarea
              value={def.httpBody ?? '{}'}
              onChange={e => set('httpBody', e.target.value)}
              rows={5}
              className="module-editor__code"
            />
          </div>
        </div>
      )}

      <div className="module-editor__section">
        <div className="module-editor__params-header">
          <h4 className="module-editor__section-title">Parameter</h4>
          <button className="btn btn--icon" onClick={addParam}>+ Parameter</button>
        </div>

        {def.parameters.length === 0 && (
          <p className="inspector__no-params">Keine Parameter definiert.</p>
        )}

        {def.parameters.map((p, i) => (
          <div key={i} className="module-editor__param-row">
            <input
              placeholder="key"
              value={p.key}
              onChange={e => updateParam(i, { key: e.target.value })}
              className="module-editor__param-key"
            />
            <input
              placeholder="Label"
              value={p.label}
              onChange={e => updateParam(i, { label: e.target.value })}
              className="module-editor__param-label"
            />
            <select value={p.type} onChange={e => updateParam(i, { type: e.target.value as ParameterSchema['type'] })}>
              <option value="text">Text</option>
              <option value="textarea">Textarea</option>
              <option value="select">Select</option>
              <option value="number">Number</option>
              <option value="toggle">Toggle</option>
            </select>
            <input
              placeholder="Default"
              value={p.default ?? ''}
              onChange={e => updateParam(i, { default: e.target.value })}
              className="module-editor__param-default"
            />
            <label className="module-editor__required">
              <input
                type="checkbox"
                checked={p.required}
                onChange={e => updateParam(i, { required: e.target.checked })}
              />
              Req
            </label>
            <button className="btn btn--icon btn--danger" onClick={() => removeParam(i)}>✕</button>
          </div>
        ))}
      </div>

      <div className="module-editor__footer">
        <button className="btn" onClick={onCancel}>Abbrechen</button>
        <button className="btn btn--primary" onClick={() => onSave(def)} disabled={!def.name.trim()}>
          Speichern
        </button>
      </div>
    </div>
  );
}
