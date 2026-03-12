import type { Workflow } from '../types';
import { createWorkflow, updateWorkflow } from '../api';
import { renderConditionRow, getConditions, clearConditions } from './ConditionsEditor';
import { renderActionRow, getActions, clearActions } from './ActionsEditor';

type OnSaved = () => void;

let editingId: string | null = null;
let onSavedCallback: OnSaved = () => {};

const conditionsContainer = () => document.getElementById('wf-conditions-container') as HTMLElement;
const actionsContainer    = () => document.getElementById('wf-actions-container') as HTMLElement;
const triggerTypeEl       = () => document.getElementById('wf-trigger-type') as HTMLSelectElement;
const triggerPathEl       = () => document.getElementById('wf-trigger-path') as HTMLInputElement;
const triggerPathLabel    = () => document.getElementById('label-trigger-path') as HTMLElement;

function currentTriggerType(): string {
  return triggerTypeEl().value;
}

function updatePathVisibility(): void {
  const needsPath = ['file_created', 'file_modified'].includes(currentTriggerType());
  const display = needsPath ? '' : 'none';
  triggerPathEl().style.display = display;
  triggerPathLabel().style.display = display;
}

export function initModal(onSaved: OnSaved): void {
  onSavedCallback = onSaved;

  triggerTypeEl().addEventListener('change', () => {
    updatePathVisibility();
    // Re-render existing conditions with updated field options
    const existing = getConditions(conditionsContainer());
    clearConditions(conditionsContainer());
    existing.forEach(c => renderConditionRow(conditionsContainer(), currentTriggerType(), c));
  });

  document.getElementById('btn-add-condition')!.addEventListener('click', () => {
    renderConditionRow(conditionsContainer(), currentTriggerType());
  });

  document.getElementById('btn-add-action')!.addEventListener('click', () => {
    renderActionRow(actionsContainer());
  });
}

export function openCreateModal(): void {
  editingId = null;
  document.getElementById('modal-title')!.textContent = 'Workflow erstellen';
  (document.getElementById('wf-name') as HTMLInputElement).value = '';
  (document.getElementById('wf-enabled') as HTMLSelectElement).value = 'true';
  triggerTypeEl().value = 'manual';
  triggerPathEl().value = '';
  (document.getElementById('wf-continue') as HTMLSelectElement).value = 'false';
  clearConditions(conditionsContainer());
  clearActions(actionsContainer());
  updatePathVisibility();
  document.getElementById('modal')!.classList.add('open');
}

export function openEditModal(w: Workflow): void {
  editingId = w.id;
  document.getElementById('modal-title')!.textContent = 'Workflow bearbeiten';
  (document.getElementById('wf-name') as HTMLInputElement).value = w.name;
  (document.getElementById('wf-enabled') as HTMLSelectElement).value = String(w.enabled);
  triggerTypeEl().value = w.trigger?.type ?? 'manual';
  triggerPathEl().value = w.trigger?.path ?? '';
  (document.getElementById('wf-continue') as HTMLSelectElement).value = String(w.continueOnError);

  clearConditions(conditionsContainer());
  w.conditions.forEach(c => renderConditionRow(conditionsContainer(), currentTriggerType(), c));

  clearActions(actionsContainer());
  w.actions.forEach(a => renderActionRow(actionsContainer(), a));

  updatePathVisibility();
  document.getElementById('modal')!.classList.add('open');
}

export function closeModal(): void {
  document.getElementById('modal')!.classList.remove('open');
}

export async function saveWorkflow(): Promise<void> {
  const payload = {
    name: (document.getElementById('wf-name') as HTMLInputElement).value,
    enabled: (document.getElementById('wf-enabled') as HTMLSelectElement).value === 'true',
    trigger: {
      type: currentTriggerType(),
      path: triggerPathEl().value || null,
    },
    continueOnError: (document.getElementById('wf-continue') as HTMLSelectElement).value === 'true',
    conditions: getConditions(conditionsContainer()),
    actions: getActions(actionsContainer()),
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
