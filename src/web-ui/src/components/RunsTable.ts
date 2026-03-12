import type { Run } from '../types';

export function renderRuns(runs: Run[]): void {
  const sorted = [...runs].sort(
    (a, b) => new Date(b.triggeredAt).getTime() - new Date(a.triggeredAt).getTime()
  );

  const tbody = document.getElementById('runs-body')!;
  tbody.innerHTML = '';

  for (const run of sorted.slice(0, 50)) {
    const tr = document.createElement('tr');

    const actionsHtml = run.actionsExecuted
      .map(a => {
        const cls = a.status === 'success' ? 'badge-success' : 'badge-failed';
        return `<div><span class="badge ${cls}">${a.type}</span> ${a.message ?? ''}</div>`;
      })
      .join('');

    const statusCls = run.status === 'success' ? 'badge-success' : 'badge-failed';

    tr.innerHTML = `
      <td style="color:#888;font-size:0.75rem">${run.workflowId}</td>
      <td>${new Date(run.triggeredAt).toLocaleString('de')}</td>
      <td><span class="badge ${statusCls}">${run.status}</span></td>
      <td>${run.conditionsMet ? 'Ja' : 'Nein'}</td>
      <td>${actionsHtml}</td>
      <td style="color:#ef9a9a">${run.error ?? ''}</td>
    `;

    tbody.appendChild(tr);
  }
}
