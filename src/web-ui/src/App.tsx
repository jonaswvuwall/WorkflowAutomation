import { useState, useCallback, useEffect } from 'react';
import { ReactFlowProvider, type Node, type ReactFlowInstance } from 'reactflow';
import { Canvas } from './components/Canvas';
import { ModulePalette } from './components/ModulePalette';
import { NodeInspector } from './components/NodeInspector';
import { NodeConfigModal } from './components/NodeConfigModal';
import { WorkflowList } from './components/WorkflowList';
import { RunsPanel } from './components/RunsPanel';
import { useModules } from './hooks/useModules';
import { useEvents } from './hooks/useWorkflows';
import { useCanvas, eventChainToFlow, type WorkflowNodeData } from './hooks/useCanvas';
import {
  fetchActions, fetchConditions,
  createEvent, updateEvent,
  createAction, updateAction,
  createCondition, updateCondition,
} from './api';
import type { EventDefinition, ActionDefinition, ConditionDefinition, StepRef } from './types';

function WorkflowEditor() {
  const { modules, loading } = useModules();
  const { events, reload, deleteEvent, runEvent, toggleEvent } = useEvents();
  const { nodes, setNodes, onNodesChange, edges, setEdges, onEdgesChange, onConnect } = useCanvas();

  const [activeEvent, setActiveEvent]     = useState<EventDefinition | null>(null);
  const [eventName, setEventName]         = useState('New Event');
  const [eventEnabled, setEventEnabled]   = useState(true);
  const [allActions, setAllActions]       = useState<ActionDefinition[]>([]);
  const [allConditions, setAllConditions] = useState<ConditionDefinition[]>([]);
  const [selectedNode, setSelectedNode]   = useState<Node<WorkflowNodeData> | null>(null);
  const [modalNode, setModalNode]         = useState<Node<WorkflowNodeData> | null>(null);
  const [runsRefresh, setRunsRefresh]     = useState(0);
  const [saving, setSaving]               = useState(false);

  const reloadActions    = useCallback(() => { fetchActions().then(setAllActions); }, []);
  const reloadConditions = useCallback(() => { fetchConditions().then(setAllConditions); }, []);

  useEffect(() => { reloadActions(); reloadConditions(); }, [reloadActions, reloadConditions]);

  const handleSelectEvent = useCallback((evt: EventDefinition) => {
    setActiveEvent(evt);
    setEventName(evt.name);
    setEventEnabled(evt.enabled);
    const { nodes: n, edges: e } = eventChainToFlow(evt, allActions, allConditions, modules);
    setNodes(n);
    setEdges(e);
    setSelectedNode(null);
  }, [allActions, allConditions, modules, setNodes, setEdges]);

  const handleNew = useCallback(() => {
    setActiveEvent(null);
    setEventName('New Event');
    setEventEnabled(true);
    setNodes([]);
    setEdges([]);
    setSelectedNode(null);
  }, [setNodes, setEdges]);

  const handleSave = useCallback(async () => {
    setSaving(true);

    // Separate edges by source handle
    const nextEdgeMap  = new Map<string, string[]>(); // event/action → targets
    const trueEdgeMap  = new Map<string, string[]>(); // condition true → targets
    const falseEdgeMap = new Map<string, string[]>(); // condition false → targets

    for (const e of edges) {
      if (e.sourceHandle === 'true') {
        const arr = trueEdgeMap.get(e.source) ?? [];
        arr.push(e.target);
        trueEdgeMap.set(e.source, arr);
      } else if (e.sourceHandle === 'false') {
        const arr = falseEdgeMap.get(e.source) ?? [];
        arr.push(e.target);
        falseEdgeMap.set(e.source, arr);
      } else {
        const arr = nextEdgeMap.get(e.source) ?? [];
        arr.push(e.target);
        nextEdgeMap.set(e.source, arr);
      }
    }

    const eventNode      = nodes.find(n => n.data.nodeType === 'event');
    const actionNodes    = nodes.filter(n => n.data.nodeType === 'action');
    const conditionNodes = nodes.filter(n => n.data.nodeType === 'condition');

    // canvas-ID → backend-ID map (built up across passes)
    const canvasToBackend = new Map<string, string>();

    // Helper: resolve canvas IDs → StepRef[] using the populated map
    const toStepRefs = (canvasIds: string[]): StepRef[] =>
      canvasIds.flatMap(cid => {
        const bId  = canvasToBackend.get(cid);
        const node = nodes.find(n => n.id === cid);
        if (!bId || !node) return [];
        return [{ id: bId, type: node.data.nodeType as 'action' | 'condition' }];
      });

    // Pass 1 — create new actions (empty nextSteps for now)
    for (const n of actionNodes) {
      if (allActions.find(a => a.id === n.id)) {
        canvasToBackend.set(n.id, n.id);
      } else {
        const created = await createAction({
          name: n.data.label, moduleId: n.data.moduleId,
          config: n.data.config, nextSteps: [],
          ui: { position: { x: n.position.x, y: n.position.y } },
        });
        canvasToBackend.set(n.id, created.id);
      }
    }

    // Pass 2 — create new conditions (empty next steps for now)
    for (const n of conditionNodes) {
      if (allConditions.find(c => c.id === n.id)) {
        canvasToBackend.set(n.id, n.id);
      } else {
        const created = await createCondition({
          name: n.data.label, moduleId: n.data.moduleId,
          config: n.data.config, trueNextSteps: [], falseNextSteps: [],
          ui: { position: { x: n.position.x, y: n.position.y } },
        });
        canvasToBackend.set(n.id, created.id);
      }
    }

    // Pass 3 — update actions with resolved nextSteps
    for (const n of actionNodes) {
      const backendId = canvasToBackend.get(n.id)!;
      await updateAction(backendId, {
        name: n.data.label, moduleId: n.data.moduleId,
        config: n.data.config,
        nextSteps: toStepRefs(nextEdgeMap.get(n.id) ?? []),
        ui: { position: { x: n.position.x, y: n.position.y } },
      });
    }

    // Pass 4 — update conditions with resolved true/false next steps
    for (const n of conditionNodes) {
      const backendId = canvasToBackend.get(n.id)!;
      await updateCondition(backendId, {
        name: n.data.label, moduleId: n.data.moduleId,
        config: n.data.config,
        trueNextSteps:  toStepRefs(trueEdgeMap.get(n.id)  ?? []),
        falseNextSteps: toStepRefs(falseEdgeMap.get(n.id) ?? []),
        ui: { position: { x: n.position.x, y: n.position.y } },
      });
    }

    // Build event firstSteps from the event node's outgoing edges
    const firstSteps = toStepRefs(eventNode ? (nextEdgeMap.get(eventNode.id) ?? []) : []);

    const eventPayload: Omit<EventDefinition, 'id'> = {
      name:       eventName,
      enabled:    eventEnabled,
      moduleId:   eventNode?.data.moduleId ?? '',
      config:     eventNode?.data.config   ?? {},
      firstSteps,
      ui: { position: eventNode ? { x: eventNode.position.x, y: eventNode.position.y } : { x: 0, y: 0 } },
    };

    if (activeEvent) {
      const updated = await updateEvent(activeEvent.id, eventPayload);
      setActiveEvent(updated);
    } else {
      const created = await createEvent(eventPayload);
      setActiveEvent(created);
    }

    reload();
    reloadActions();
    reloadConditions();
    setSaving(false);
  }, [nodes, edges, eventName, eventEnabled, activeEvent, allActions, allConditions, reload, reloadActions, reloadConditions]);

  const handleDrop = useCallback((e: React.DragEvent, rfInstance: ReactFlowInstance) => {
    const moduleId   = e.dataTransfer.getData('application/wf-module-id');
    const nodeType   = e.dataTransfer.getData('application/wf-node-type') as 'event' | 'action' | 'condition';
    const moduleName = e.dataTransfer.getData('application/wf-module-name');
    if (!moduleId) return;

    const allManifests = [...modules.events, ...modules.conditions, ...modules.actions];
    const manifest = allManifests.find(m => m.id === moduleId);
    const defaultConfig: Record<string, string> = {};
    manifest?.parameters.forEach(p => {
      if (p.default !== undefined) defaultConfig[p.key] = p.default;
    });

    const position = rfInstance.screenToFlowPosition({ x: e.clientX, y: e.clientY });
    const newNode: Node<WorkflowNodeData> = {
      id:       crypto.randomUUID(),
      type:     nodeType,
      position,
      data: { label: moduleName, moduleId, config: defaultConfig, nodeType },
    };
    setNodes(nds => [...nds, newNode]);
  }, [setNodes, modules]);

  const handleRun = useCallback(async (id: string) => {
    await runEvent(id);
    setRunsRefresh(r => r + 1);
  }, [runEvent]);

  if (loading) return <div className="loading">Module werden geladen…</div>;

  return (
    <div className="app">
      <WorkflowList
        events={events}
        activeId={activeEvent?.id ?? null}
        onSelect={handleSelectEvent}
        onNew={handleNew}
        onDelete={deleteEvent}
        onRun={handleRun}
        onToggle={toggleEvent}
      />

      <div className="app__center">
        <div className="toolbar">
          <input
            className="toolbar__name"
            value={eventName}
            onChange={e => setEventName(e.target.value)}
            placeholder="Event Name"
          />
          <label className="toolbar__toggle">
            <input
              type="checkbox"
              checked={eventEnabled}
              onChange={e => setEventEnabled(e.target.checked)}
            />
            Enabled
          </label>
          <button className="btn btn--primary" onClick={handleSave} disabled={saving}>
            {saving ? 'Speichern…' : 'Speichern'}
          </button>
        </div>

        <Canvas
          nodes={nodes}
          edges={edges}
          onNodesChange={onNodesChange}
          onEdgesChange={onEdgesChange}
          onConnect={onConnect}
          onNodeClick={setSelectedNode}
          onNodeDoubleClick={setModalNode}
          onDrop={handleDrop}
          modules={modules}
        />

        <RunsPanel refreshTrigger={runsRefresh} />
      </div>

      <div className="app__right">
        <ModulePalette modules={modules} />
        <NodeInspector
          node={nodes.find(n => n.id === selectedNode?.id) ?? selectedNode}
          modules={modules}
          onDeselect={() => setSelectedNode(null)}
        />
      </div>

      {modalNode && (
        <NodeConfigModal
          node={modalNode}
          modules={modules}
          onClose={() => setModalNode(null)}
        />
      )}
    </div>
  );
}

export default function App() {
  return (
    <ReactFlowProvider>
      <WorkflowEditor />
    </ReactFlowProvider>
  );
}
