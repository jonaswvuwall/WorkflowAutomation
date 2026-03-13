import { useState, useEffect, useCallback } from 'react';
import { fetchEvents, deleteEvent as apiDelete, runEvent as apiRun, toggleEvent as apiToggle } from '../api';
import type { EventDefinition } from '../types';

export function useEvents() {
  const [events, setEvents] = useState<EventDefinition[]>([]);

  const reload = useCallback(() => {
    fetchEvents().then(setEvents);
  }, []);

  useEffect(() => { reload(); }, [reload]);

  const deleteEvent = useCallback(async (id: string) => {
    await apiDelete(id);
    reload();
  }, [reload]);

  const runEvent = useCallback(async (id: string) => {
    return apiRun(id);
  }, []);

  const toggleEvent = useCallback(async (id: string) => {
    const updated = await apiToggle(id);
    setEvents(evts => evts.map(e => e.id === id ? updated : e));
    return updated;
  }, []);

  return { events, reload, deleteEvent, runEvent, toggleEvent };
}
