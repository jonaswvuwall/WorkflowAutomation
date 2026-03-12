import type { Workflow } from '../types';

export interface WorkflowTableCallbacks {
  onRun: (id: string) => void;
  onEdit: (workflow: Workflow) => void;
  onDelete: (id: string) => void;
}

export function renderWorkflows(workflows: Workflow[], callbacks: WorkflowTableCallbacks): void {
  const tbody = document.getElementById('workflows-body')!;
  tbody.innerHTML = '';

  for (const w of workflows) {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${w.name}</td>
      <td>${w.when.type}<br><small style="color:#888">${w.when.path ?? ''}</small></td>
      <td>${w.then.type || '–'}</td>
      <td><span class="badge ${w.enabled ? 'badge-enabled' : 'badge-disabled'}">${w.enabled ? 'aktiv' : 'inaktiv'}</span></td>
      <td>${new Date(w.createdAt).toLocaleString('de')}</td>
      <td>
        <div class="actions-cell">
          <button data-action="run">▶ Run</button>
          <button data-action="edit">Bearbeiten</button>
          <button class="danger" data-action="delete">Löschen</button>
        </div>
      </td>
    `;

    tr.querySelector<HTMLButtonElement>('[data-action="run"]')!
      .addEventListener('click', () => callbacks.onRun(w.id));
    tr.querySelector<HTMLButtonElement>('[data-action="edit"]')!
      .addEventListener('click', () => callbacks.onEdit(w));
    tr.querySelector<HTMLButtonElement>('[data-action="delete"]')!
      .addEventListener('click', () => callbacks.onDelete(w.id));

    tbody.appendChild(tr);
  }
}
