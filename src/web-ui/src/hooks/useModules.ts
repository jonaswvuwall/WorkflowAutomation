import { useState, useEffect, useCallback } from 'react';
import { fetchModules } from '../api';
import type { ModulesResponse } from '../types';

const empty: ModulesResponse = { events: [], actions: [] };

export function useModules() {
  const [modules, setModules] = useState<ModulesResponse>(empty);
  const [loading, setLoading] = useState(true);

  const reload = useCallback(() => {
    fetchModules().then(m => { setModules(m); setLoading(false); });
  }, []);

  useEffect(() => { reload(); }, [reload]);

  return { modules, loading, reload };
}
