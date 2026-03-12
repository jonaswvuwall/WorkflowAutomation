import type { Workflow } from '../types';
import { createWorkflow, updateWorkflow } from '../api';

type OnSaved = () => void;

let editingId: string | null = null;
let onSavedCallback: OnSaved = () => {};

export function initModal(onSaved: OnSaved): void {
  onSavedCallback = onSaved;
}

export function openCreateModal(): void {
  editingId = null;
  document.getElementById('modal-title')!.textContent = 'Workflow erstellen';
  (document.getElementById('wf-name') as HTMLInputElement).value = '';
  (document.getElementById('wf-enabled') as HTMLSelectElement).value = 'true';
  (document.getElementById('wf-trigger-type') as HTMLInputElement).value = '';
  (document.getElementById('wf-trigger-path') as HTMLInputElement).value = '';
  (document.getElementById('wf-continue') as HTMLSelectElement).value = 'false';
  (document.getElementById('wf-conditions') as HTMLTextAreaElement).value = '[]';
  (document.getElementById('wf-actions') as HTMLTextAreaElement).value = '[]';
  document.getElementById('modal')!.classList.add('open');
}

export function openEditModal(w: Workflow): void {
  editingId = w.id;
  document.getElementById('modal-title')!.textContent = 'Workflow bearbeiten';
  (document.getElementById('wf-name') as HTMLInputElement).value = w.name;
  (document.getElementById('wf-enabled') as HTMLSelectElement).value = String(w.enabled);
  (document.getElementById('wf-trigger-type') as HTMLInputElement).value = w.trigger?.type ?? '';
  (document.getElementById('wf-trigger-path') as HTMLInputElement).value = w.trigger?.path ?? '';
  (document.getElementById('wf-continue') as HTMLSelectElement).value = String(w.continueOnError);
  (document.getElementById('wf-conditions') as HTMLTextAreaElement).value = JSON.stringify(w.conditions, null, 2);
  (document.getElementById('wf-actions') as HTMLTextAreaElement).value = JSON.stringify(w.actions, null, 2);
  document.getElementById('modal')!.classList.add('open');
}

export function closeModal(): void {
  document.getElementById('modal')!.classList.remove('open');
}

export async function saveWorkflow(): Promise<void> {
  let conditions: unknown[], actions: unknown[];
  try {
    conditions = JSON.parse((document.getElementById('wf-conditions') as HTMLTextAreaElement).value);
    actions = JSON.parse((document.getElementById('wf-actions') as HTMLTextAreaElement).value);
  } catch {
    alert('Conditions oder Actions sind kein gültiges JSON');
    return;
  }

  const payload = {
    name: (document.getElementById('wf-name') as HTMLInputElement).value,
    enabled: (document.getElementById('wf-enabled') as HTMLSelectElement).value === 'true',
    trigger: {
      type: (document.getElementById('wf-trigger-type') as HTMLInputElement).value,
      path: (document.getElementById('wf-trigger-path') as HTMLInputElement).value || null,
    },
    continueOnError: (document.getElementById('wf-continue') as HTMLSelectElement).value === 'true',
    conditions,
    actions,
  };

  const ok = editingId
    ? await updateWorkflow(editingId, payload)
    : await createWorkflow(payload);

  if (ok) {
    closeModal();
    onSavedCallback();
  } else {
    alert('Fehler beim Speichern');
  }
}
