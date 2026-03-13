import type { ModulesResponse, ModuleManifest } from '../types';

interface ModulePaletteProps {
  modules: ModulesResponse;
}

function PaletteItem({ manifest, nodeType }: { manifest: ModuleManifest; nodeType: string }) {
  const onDragStart = (e: React.DragEvent) => {
    e.dataTransfer.setData('application/wf-module-id',   manifest.id);
    e.dataTransfer.setData('application/wf-node-type',   nodeType);
    e.dataTransfer.setData('application/wf-module-name', manifest.name);
    e.dataTransfer.effectAllowed = 'move';
  };

  return (
    <div
      className={`palette-item palette-item--${nodeType}`}
      draggable
      onDragStart={onDragStart}
      title={manifest.description}
    >
      <span className="palette-item__name">{manifest.name}</span>
      <span className="palette-item__category">{manifest.category}</span>
    </div>
  );
}

export function ModulePalette({ modules }: ModulePaletteProps) {
  return (
    <aside className="palette">
      <h3 className="palette__title">Modules</h3>

      <section className="palette__section">
        <h4 className="palette__section-title palette__section-title--event">Events</h4>
        {modules.events.map(m => (
          <PaletteItem key={m.id} manifest={m} nodeType="event" />
        ))}
      </section>

      <section className="palette__section">
        <h4 className="palette__section-title palette__section-title--action">Actions</h4>
        {modules.actions.map(m => (
          <PaletteItem key={m.id} manifest={m} nodeType="action" />
        ))}
      </section>
    </aside>
  );
}
