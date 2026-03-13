import { useState, useEffect } from 'react';
import { fetchRuns } from '../api';
import type { Run } from '../types';

interface RunsPanelProps {
  refreshTrigger: number;
}

export function RunsPanel({ refreshTrigger }: RunsPanelProps) {
  const [runs, setRuns] = useState<Run[]>([]);

  useEffect(() => {
    fetchRuns().then(r => setRuns(r.slice(-20).reverse()));
  }, [refreshTrigger]);

  return (
    <div className="runs-panel">
      <h3 className="runs-panel__title">Letzte Runs</h3>
      {runs.length === 0 && <p className="runs-panel__empty">Noch keine Runs.</p>}
      <ul className="runs-panel__list">
        {runs.map(run => (
          <li key={run.id} className={`runs-panel__item runs-panel__item--${run.status}`}>
            <div className="runs-panel__meta">
              <span className={`runs-panel__badge runs-panel__badge--${run.status}`}>
                {run.status}
              </span>
              <span className="runs-panel__time">
                {new Date(run.triggeredAt).toLocaleTimeString('de-DE')}
              </span>
            </div>
            <div className="runs-panel__workflow">{run.eventName}</div>
            {run.actionResults.map((r, i) => (
              <div key={i} className={`runs-panel__result runs-panel__result--${r.status}`}>
                {r.moduleId}: {r.message}
              </div>
            ))}
          </li>
        ))}
      </ul>
    </div>
  );
}
