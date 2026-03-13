import { useState } from 'react';
import { ModuleEditor } from './ModuleEditor';
import { useCustomModules } from '../hooks/useCustomModules';
import type { CustomModuleDefinition } from '../types';

interface ModuleManagerProps {
  onModulesChanged: () => void;
}

export function ModuleManager({ onModulesChanged }: ModuleManagerProps) {
  const { customModules, create, update, remove } = useCustomModules(onModulesChanged);
  const [editing, setEditing] = useState<CustomModuleDefinition | null | 'new'>(null);

  const handleSave = async (def: Omit<CustomModuleDefinition, 'id'>) => {
    if (editing === 'new') {
      await create(def);
    } else if (editing) {
      await update(editing.id, def);
    }
    setEditing(null);
  };

  if (editing !== null) {
    return (
      <div className="module-manager">
        <div className="module-manager__header">
          <h2 className="module-manager__title">
            {editing === 'new' ? 'Neues Modul' : `Modul bearbeiten: ${editing.name}`}
          </h2>
        </div>
        <ModuleEditor
          initial={editing === 'new' ? undefined : editing}
          onSave={handleSave}
          onCancel={() => setEditing(null)}
        />
      </div>
    );
  }

  return (
    <div className="module-manager">
      <div className="module-manager__header">
        <h2 className="module-manager__title">Custom Modules</h2>
        <button className="btn btn--primary" onClick={() => setEditing('new')}>
          + Neues Modul
        </button>
      </div>

      {customModules.length === 0 ? (
        <p className="module-manager__empty">
          Noch keine Custom-Module definiert. Klicke auf "+ Neues Modul" um zu starten.
        </p>
      ) : (
        <table className="module-manager__table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Typ</th>
              <th>Base</th>
              <th>Kategorie</th>
              <th>ID</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {customModules.map(m => (
              <tr key={m.id}>
                <td><strong>{m.name}</strong><br /><span className="module-manager__desc">{m.description}</span></td>
                <td><span className={`module-manager__badge module-manager__badge--${m.moduleType}`}>{m.moduleType}</span></td>
                <td>{m.baseType === 'script' ? 'Script' : 'HTTP'}</td>
                <td>{m.category}</td>
                <td><code className="module-manager__id">{m.id}</code></td>
                <td className="module-manager__actions">
                  <button className="btn btn--icon" onClick={() => setEditing(m)}>Bearbeiten</button>
                  <button className="btn btn--icon btn--danger" onClick={() => remove(m.id)}>Löschen</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
