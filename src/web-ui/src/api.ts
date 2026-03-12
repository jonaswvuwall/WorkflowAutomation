import type { Workflow, Run, StatusData } from './types';

const API = 'http://localhost:5000/api';

export async function fetchStatus(): Promise<StatusData> {
  const r = await fetch(`${API}/status`);
  return r.json();
}

export async function fetchWorkflows(): Promise<Workflow[]> {
  const r = await fetch(`${API}/workflows`);
  return r.json();
}

export async function fetchRuns(): Promise<Run[]> {
  const r = await fetch(`${API}/runs`);
  return r.json();
}

export async function runWorkflow(id: string): Promise<boolean> {
  const r = await fetch(`${API}/workflows/${id}/run`, { method: 'POST' });
  return r.ok;
}

export async function deleteWorkflow(id: string): Promise<void> {
  await fetch(`${API}/workflows/${id}`, { method: 'DELETE' });
}

export async function createWorkflow(payload: Omit<Workflow, 'id' | 'createdAt'>): Promise<boolean> {
  const r = await fetch(`${API}/workflows`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  return r.ok;
}

export async function updateWorkflow(id: string, payload: Omit<Workflow, 'id' | 'createdAt'>): Promise<boolean> {
  const r = await fetch(`${API}/workflows/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  return r.ok;
}
