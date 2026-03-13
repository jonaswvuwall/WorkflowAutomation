import type { EventDefinition, ActionDefinition, Run, StatusData, ModulesResponse, CustomModuleDefinition } from './types';

const API = 'http://localhost:5000/api';

export async function fetchStatus(): Promise<StatusData> {
  const r = await fetch(`${API}/status`);
  return r.json();
}

export async function fetchModules(): Promise<ModulesResponse> {
  const r = await fetch(`${API}/modules`);
  return r.json();
}

// ── Events ────────────────────────────────────────────────────────────────

export async function fetchEvents(): Promise<EventDefinition[]> {
  const r = await fetch(`${API}/events`);
  return r.json();
}

export async function createEvent(evt: Omit<EventDefinition, 'id'>): Promise<EventDefinition> {
  const r = await fetch(`${API}/events`, {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(evt),
  });
  return r.json();
}

export async function updateEvent(id: string, evt: Omit<EventDefinition, 'id'>): Promise<EventDefinition> {
  const r = await fetch(`${API}/events/${id}`, {
    method:  'PUT',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(evt),
  });
  return r.json();
}

export async function deleteEvent(id: string): Promise<void> {
  await fetch(`${API}/events/${id}`, { method: 'DELETE' });
}

export async function runEvent(id: string): Promise<Run> {
  const r = await fetch(`${API}/events/${id}/run`, { method: 'POST' });
  return r.json();
}

export async function toggleEvent(id: string): Promise<EventDefinition> {
  const r = await fetch(`${API}/events/${id}/toggle`, { method: 'POST' });
  return r.json();
}

// ── Actions ───────────────────────────────────────────────────────────────

export async function fetchActions(): Promise<ActionDefinition[]> {
  const r = await fetch(`${API}/actions`);
  return r.json();
}

export async function createAction(action: Omit<ActionDefinition, 'id'>): Promise<ActionDefinition> {
  const r = await fetch(`${API}/actions`, {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(action),
  });
  return r.json();
}

export async function updateAction(id: string, action: Omit<ActionDefinition, 'id'>): Promise<ActionDefinition> {
  const r = await fetch(`${API}/actions/${id}`, {
    method:  'PUT',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(action),
  });
  return r.json();
}

export async function deleteAction(id: string): Promise<void> {
  await fetch(`${API}/actions/${id}`, { method: 'DELETE' });
}

// ── Runs ──────────────────────────────────────────────────────────────────

export async function fetchRuns(): Promise<Run[]> {
  const r = await fetch(`${API}/runs`);
  return r.json();
}

// ── Custom Modules ────────────────────────────────────────────────────────

export async function fetchCustomModules(): Promise<CustomModuleDefinition[]> {
  const r = await fetch(`${API}/custom-modules`);
  return r.json();
}

export async function createCustomModule(def: Omit<CustomModuleDefinition, 'id'>): Promise<CustomModuleDefinition> {
  const r = await fetch(`${API}/custom-modules`, {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(def),
  });
  return r.json();
}

export async function updateCustomModule(id: string, def: Omit<CustomModuleDefinition, 'id'>): Promise<CustomModuleDefinition> {
  const r = await fetch(`${API}/custom-modules/${id}`, {
    method:  'PUT',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify(def),
  });
  return r.json();
}

export async function deleteCustomModule(id: string): Promise<void> {
  await fetch(`${API}/custom-modules/${id}`, { method: 'DELETE' });
}
