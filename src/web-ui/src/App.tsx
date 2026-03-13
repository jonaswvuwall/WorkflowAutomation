import { useState, useCallback, useEffect } from 'react';
import { ReactFlowProvider, type Node, type ReactFlowInstance } from 'reactflow';
import { Canvas } from './components/Canvas';
import { ModulePalette } from './components/ModulePalette';
import { NodeInspector } from './components/NodeInspector';
import { NodeConfigModal } from './components/NodeConfigModal';
import { WorkflowList } from './components/WorkflowList';
import { RunsPanel } from './components/RunsPanel';
import { ModuleManager } from './components/ModuleManager';
import { useModules } from './hooks/useModules';
import { useEvents } from './hooks/useWorkflows';
import { useCanvas, eventChainToFlow, type WorkflowNodeData } from './hooks/useCanvas';
import { fetchActions, createEvent, updateEvent, createAction, updateAction } from './api';
import type { EventDefinition, ActionDefinition } from './types';

type View = 'canvas' | 'modules';

function WorkflowEditor() {
  const { modules, loading, reload: reloadModules } = useModules();
  const { events, reload, deleteEvent, runEvent, toggleEvent } = useEvents();
  const { nodes, setNodes, onNodesChange, edges, setEdges, onEdgesChange, onConnect } = useCanvas();

  const [view, setView]                         = useState<View>('canvas');
  const [activeEvent, setActiveEvent]           = useState<EventDefinition | null>(null);
  const [eventName, setEventName]               = useState('New Event');
  const [eventEnabled, setEventEnabled]         = useState(true);
  const [allActions, setAllActions]             = useState<ActionDefinition[]>([]);
  const [selectedNode, setSelectedNode]         = useState<Node<WorkflowNodeData> | null>(null);
  const [modalNode, setModalNode]               = useState<Node<WorkflowNodeData> | null>(null);
  const [runsRefresh, setRunsRefresh]           = useState(0);
  const [saving, setSaving]                     = useState(false);

  // Keep allActions in sync
  const reloadActions = useCallback(() => {
    fetchActions().then(setAllActions);
  }, []);

  useEffect(() => { reloadActions(); }, [reloadActions]);

  const handleSelectEvent = useCallback((evt: EventDefinition) => {
    setActiveEvent(evt);
    setEventName(evt.name);
    setEventEnabled(evt.enabled);
    const { nodes: n, edges: e } = eventChainToFlow(evt, allActions, modules);
    setNodes(n);
    setEdges(e);
    setSelectedNode(null);
  }, [allActions, modules, setNodes, setEdges]);

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

    // Build edge map: sourceId → targetId
    const edgeMap = new Map<string, string>(edges.map(e => [e.source, e.target]));

    const eventNode   = nodes.find(n => n.data.nodeType === 'event');
    const actionNodes = nodes.filter(n => n.data.nodeType === 'action');

    // Save / update all action nodes
    const savedActions: ActionDefinition[] = [];
    for (const n of actionNodes) {
      const actionPayload: Omit<ActionDefinition, 'id'> = {
        name:         n.data.label,
        moduleId:     n.data.moduleId,
        config:       n.data.config,
        nextActionId: edgeMap.get(n.id) ?? null,
        position:     { x: n.position.x, y: n.position.y },
      };
      const exists = allActions.find(a => a.id === n.id);
      if (exists) {
        const updated = await updateAction(n.id, actionPayload);
        savedActions.push(updated);
      } else {
        const created = await createAction(actionPayload);
        savedActions.push(created);
      }
    }

    // Determine firstActionId: target of the event node's outgoing edge
    const firstActionId = eventNode ? (edgeMap.get(eventNode.id) ?? null) : null;

    const eventPayload: Omit<EventDefinition, 'id'> = {
      name:          eventName,
      enabled:       eventEnabled,
      moduleId:      eventNode?.data.moduleId ?? '',
      config:        eventNode?.data.config   ?? {},
      firstActionId,
      position:      eventNode ? { x: eventNode.position.x, y: eventNode.position.y } : { x: 0, y: 0 },
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
    setSaving(false);
  }, [nodes, edges, eventName, eventEnabled, activeEvent, allActions, reload, reloadActions]);

  const handleDrop = useCallback((e: React.DragEvent, rfInstance: ReactFlowInstance) => {
    const moduleId   = e.dataTransfer.getData('application/wf-module-id');
    const nodeType   = e.dataTransfer.getData('application/wf-node-type') as 'event' | 'action';
    const moduleName = e.dataTransfer.getData('application/wf-module-name');
    if (!moduleId) return;

    // Pre-fill config from template defaults
    const allManifests = [...modules.events, ...modules.actions];
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
      {/* Left: Event list */}
      <WorkflowList
        events={events}
        activeId={activeEvent?.id ?? null}
        onSelect={handleSelectEvent}
        onNew={handleNew}
        onDelete={deleteEvent}
        onRun={handleRun}
        onToggle={toggleEvent}
      />

      {/* Center */}
      <div className="app__center">
        <div className="tab-bar">
          <button
            className={`tab-bar__tab ${view === 'canvas' ? 'tab-bar__tab--active' : ''}`}
            onClick={() => setView('canvas')}
          >
            Events
          </button>
          <button
            className={`tab-bar__tab ${view === 'modules' ? 'tab-bar__tab--active' : ''}`}
            onClick={() => setView('modules')}
          >
            Module Manager
          </button>
        </div>

        {view === 'canvas' ? (
          <>
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
          </>
        ) : (
          <ModuleManager onModulesChanged={reloadModules} />
        )}
      </div>

      {/* Right: Module palette + node inspector (only in canvas view) */}
      {view === 'canvas' && (
        <div className="app__right">
          <ModulePalette modules={modules} />
          <NodeInspector
            node={selectedNode}
            modules={modules}
            onDeselect={() => setSelectedNode(null)}
          />
        </div>
      )}

      {/* Double-click modal */}
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
