import type { StatusData } from '../types';

export function updateStatusBar(data: StatusData): void {
  document.getElementById('s-status')!.textContent = data.status;
  document.getElementById('s-workflows')!.textContent =
    `${data.workflows.enabled} aktiv / ${data.workflows.total} gesamt`;
  document.getElementById('s-runs')!.textContent = String(data.runs.total);
  document.getElementById('s-success')!.textContent = String(data.runs.success);
  document.getElementById('s-failed')!.textContent = String(data.runs.failed);
}
