import { useState, useEffect, useCallback } from 'react';
import { fetchCustomModules, createCustomModule, updateCustomModule, deleteCustomModule } from '../api';
import type { CustomModuleDefinition } from '../types';

export function useCustomModules(onModulesChanged?: () => void) {
  const [customModules, setCustomModules] = useState<CustomModuleDefinition[]>([]);
  const [loading, setLoading] = useState(false);

  const reload = useCallback(() => {
    fetchCustomModules().then(setCustomModules);
  }, []);

  useEffect(() => { reload(); }, [reload]);

  const create = useCallback(async (def: Omit<CustomModuleDefinition, 'id'>) => {
    setLoading(true);
    await createCustomModule(def);
    reload();
    onModulesChanged?.();
    setLoading(false);
  }, [reload, onModulesChanged]);

  const update = useCallback(async (id: string, def: Omit<CustomModuleDefinition, 'id'>) => {
    setLoading(true);
    await updateCustomModule(id, def);
    reload();
    onModulesChanged?.();
    setLoading(false);
  }, [reload, onModulesChanged]);

  const remove = useCallback(async (id: string) => {
    setLoading(true);
    await deleteCustomModule(id);
    reload();
    onModulesChanged?.();
    setLoading(false);
  }, [reload, onModulesChanged]);

  return { customModules, loading, reload, create, update, remove };
}
