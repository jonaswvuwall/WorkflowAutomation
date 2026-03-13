import type { EventDefinition } from '../types';

interface EventListProps {
  events:   EventDefinition[];
  activeId: string | null;
  onSelect: (event: EventDefinition) => void;
  onNew:    () => void;
  onDelete: (id: string) => void;
  onRun:    (id: string) => void;
  onToggle: (id: string) => void;
}

export function WorkflowList({ events, activeId, onSelect, onNew, onDelete, onRun, onToggle }: EventListProps) {
  return (
    <aside className="workflow-list">
      <div className="workflow-list__header">
        <h3>Events</h3>
        <button className="btn btn--primary" onClick={onNew}>+ Neu</button>
      </div>
      <ul className="workflow-list__items">
        {events.length === 0 && (
          <li className="workflow-list__empty">Noch keine Events</li>
        )}
        {events.map(evt => (
          <li
            key={evt.id}
            className={`workflow-list__item ${activeId === evt.id ? 'workflow-list__item--active' : ''}`}
            onClick={() => onSelect(evt)}
          >
            <div className="workflow-list__item-name">
              <span className={`workflow-list__dot ${evt.enabled ? 'workflow-list__dot--on' : 'workflow-list__dot--off'}`} />
              {evt.name}
            </div>
            <div className="workflow-list__actions" onClick={e => e.stopPropagation()}>
              <button className="btn btn--icon" title={evt.enabled ? 'Deaktivieren' : 'Aktivieren'} onClick={() => onToggle(evt.id)}>
                {evt.enabled ? '⏸' : '▷'}
              </button>
              <button className="btn btn--icon" title="Run" onClick={() => onRun(evt.id)}>▶</button>
              <button className="btn btn--icon btn--danger" title="Löschen" onClick={() => {
                if (confirm(`Event "${evt.name}" löschen?`)) onDelete(evt.id);
              }}>✕</button>
            </div>
          </li>
        ))}
      </ul>
    </aside>
  );
}
