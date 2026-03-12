import './styles/main.css';
import { fetchStatus, fetchWorkflows, fetchRuns, runWorkflow, deleteWorkflow } from './api';
import { updateStatusBar } from './components/StatusBar';
import { renderWorkflows } from './components/WorkflowTable';
import { renderRuns } from './components/RunsTable';
import { initModal, openCreateModal, openEditModal, closeModal, saveWorkflow } from './components/WorkflowModal';

async function loadAll(): Promise<void> {
  await Promise.all([loadStatus(), loadWorkflows(), loadRuns()]);
}

async function loadStatus(): Promise<void> {
  const data = await fetchStatus();
  updateStatusBar(data);
}

async function loadWorkflows(): Promise<void> {
  const workflows = await fetchWorkflows();
  renderWorkflows(workflows, {
    onRun: async (id) => {
      const ok = await runWorkflow(id);
      if (ok) await loadAll();
      else alert('Run fehlgeschlagen');
    },
    onEdit: (workflow) => openEditModal(workflow),
    onDelete: async (id) => {
      if (!confirm('Workflow löschen?')) return;
      await deleteWorkflow(id);
      await loadAll();
    },
  });
}

async function loadRuns(): Promise<void> {
  const runs = await fetchRuns();
  renderRuns(runs);
}

// Wire up static buttons
document.getElementById('btn-refresh')!.addEventListener('click', () => loadAll());
document.getElementById('btn-new-workflow')!.addEventListener('click', () => openCreateModal());
document.getElementById('btn-modal-cancel')!.addEventListener('click', () => closeModal());
document.getElementById('btn-modal-save')!.addEventListener('click', () => saveWorkflow());

initModal(() => loadAll());
loadAll();
