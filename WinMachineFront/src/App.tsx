import { useState, useEffect, useContext } from 'react';
import { machines as initialMachines } from './data/mockMachines';
// import { DeviceNode } from './components/DeviceNode'; // Replaced by 3D
import type { MachineSchema } from './types';
import { TelemetryProvider, useTelemetryClient, useTelemetryObservable, TelemetryContext } from './services/telemetry/TelemetryContext';
import { MachineService } from './services/machine/MachineService';
import { Canvas, useThree } from '@react-three/fiber';
import { Stage, OrbitControls } from '@react-three/drei';
import { DeviceNode3D } from './components/DeviceNode3D';
import { Vector3 } from 'three';

function StepIndicator() {
  const observable = useTelemetryObservable();
  const [step, setStep] = useState('IDLE');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    return observable.subscribe(p => {
      if (p.step) setStep(p.step);
      // Display latest error if any
      const errorEvent = p.e?.find(evt => evt.type === 'Error');
      if (errorEvent) {
        setError(errorEvent.msg);
        setTimeout(() => setError(null), 5000); // Clear error after 5s
      }
    });
  }, [observable]);

  return (
    <div className="flex flex-col gap-1">
      <div className="flex items-center gap-2 px-2 py-1 bg-amber-950/30 border border-amber-900/50 rounded text-amber-400 text-xs font-mono animate-in fade-in slide-in-from-top-2">
        <span className="opacity-50">STEP:</span>
        <span className="font-bold uppercase tracking-widest">{step}</span>
      </div>
      {error && (
        <div className="px-2 py-1 bg-red-950/50 border border-red-500/50 rounded text-red-400 text-[10px] font-mono animate-bounce">
          {error}
        </div>
      )}
    </div>
  );
}

function ConnectionStatus() {
  const telemetryClient = useTelemetryClient();
  const [status, setStatus] = useState<'CONNECTED' | 'DISCONNECTED' | 'CONNECTING'>('CONNECTING');

  useEffect(() => {
    const timer = setInterval(() => {
      if ((telemetryClient as any).isConnected) {
        setStatus('CONNECTED');
      } else if ((telemetryClient as any).socket?.readyState === 0) {
        setStatus('CONNECTING');
      } else {
        setStatus('DISCONNECTED');
      }
    }, 1000);
    return () => clearInterval(timer);
  }, [telemetryClient]);

  const colors = {
    CONNECTED: 'text-green-500 bg-green-500/10 border-green-500/20',
    DISCONNECTED: 'text-red-500 bg-red-500/10 border-red-500/20',
    CONNECTING: 'text-amber-500 bg-amber-500/10 border-amber-500/20'
  };

  return (
    <div className={`px-2 py-0.5 rounded border text-[10px] font-mono font-bold tracking-tighter ${colors[status]}`}>
      {status}
    </div>
  );
}

const INITIAL_CAMERA_POS_V3 = new Vector3(1000, -1000, 1000);
const INITIAL_TARGET_V3 = new Vector3(0, 0, 0);

function CameraController({ trigger }: { trigger: number }) {
  const { camera, controls, size } = useThree();

  useEffect(() => {
    if (trigger > 0) {
      // For Orthographic Reset
      camera.position.copy(INITIAL_CAMERA_POS_V3);
      camera.up.set(0, 0, 1); // Enforce Z-up
      camera.lookAt(INITIAL_TARGET_V3);

      // Reset Zoom - Orthographic zoom is different from perspective dist
      if ((camera as any).isOrthographicCamera) {
        camera.zoom = 1;
        camera.updateProjectionMatrix();
      }

      if (controls) {
        (controls as any).target.copy(INITIAL_TARGET_V3);
        (controls as any).update();
      }
    }
  }, [trigger, camera, controls]);

  return null;
}

function MachineView() {
  const [scenarios, setScenarios] = useState<string[]>([]);
  const [selectedScenarioName, setSelectedScenarioName] = useState<string>('');
  const [selectedMachine, setSelectedMachine] = useState<MachineSchema | null>(null);
  // const [zoom, setZoom] = useState(1); // Handle by OrbitControls
  const [loading, setLoading] = useState(true);
  const [resetTrigger, setResetTrigger] = useState(0);

  const telemetryClient = useTelemetryClient();
  const telemetryCtx = useContext(TelemetryContext);

  // Load scenarios on mount
  useEffect(() => {
    MachineService.getScenarios().then(list => {
      setScenarios(list);
      if (list.length > 0) {
        setSelectedScenarioName(list[0]);
      }
      setLoading(false);
    });
  }, []);

  // Load schema when scenario name changes
  useEffect(() => {
    if (!selectedScenarioName) return;
    setLoading(true);
    MachineService.getSchema(selectedScenarioName).then(schema => {
      if (schema) {
        setSelectedMachine(schema);
      } else {
        // Fallback to initialMachines if API fails
        const fallback = initialMachines.find(m => m.machineName === selectedScenarioName) || initialMachines[0];
        setSelectedMachine(fallback);
      }
      setLoading(false);
    });
  }, [selectedScenarioName]);

  const handleStart = () => {
    if (selectedScenarioName) {
      telemetryClient.startFlow(selectedScenarioName);
    }
  };

  const handleStop = () => {
    telemetryClient.stopFlow();
  };

  if (!selectedMachine && loading) {
    return (
      <div className="w-full h-screen bg-neutral-950 flex items-center justify-center font-mono text-cyan-500">
        <div className="animate-pulse">LOADING MACHINE SCHEMA...</div>
      </div>
    );
  }

  return (
    <div className="flex w-full h-screen bg-neutral-900 text-slate-200 overflow-hidden font-sans selection:bg-cyan-500/30">

      {/* Sidebar */}
      <div className="w-80 bg-slate-900 border-r border-slate-700 flex flex-col z-20 shadow-2xl">
        <div className="p-4 border-b border-slate-700 bg-slate-900/50 backdrop-blur-sm">
          <h2 className="text-xl font-bold font-mono text-cyan-500 tracking-wider flex items-center gap-2">
            <div className="w-2 h-2 bg-cyan-500 rounded-sm animate-pulse" />
            WINMACHINE
          </h2>
          <p className="text-xs text-slate-500 mt-1 font-mono">SCENARIO EXPLORER v2.0</p>
        </div>

        <div className="flex-1 overflow-y-auto p-2 space-y-2">
          {scenarios.map((name) => (
            <button
              key={name}
              onClick={() => setSelectedScenarioName(name)}
              className={`w-full text-left p-3 rounded-md transition-all border font-mono text-sm relative group
                ${selectedScenarioName === name
                  ? 'bg-slate-800 border-cyan-500/50 text-cyan-400 shadow-[0_0_15px_rgba(6,182,212,0.1)]'
                  : 'bg-transparent border-transparent hover:bg-slate-800/50 text-slate-400 hover:text-slate-200'
                }
              `}
            >
              {selectedScenarioName === name && (
                <div className="absolute left-0 top-0 bottom-0 w-[2px] bg-cyan-500 rounded-l-md" />
              )}
              <div className="font-bold truncate">{name.replace(/_/g, ' ')}</div>
            </button>
          ))}
        </div>

        {/* Telemetry Controls */}
        <div className="p-4 border-t border-slate-800 bg-slate-900/80">
          <h3 className="text-xs font-bold text-slate-400 mb-2 font-mono uppercase">Control Panel</h3>
          <div className="grid grid-cols-2 gap-2">
            <button
              onClick={handleStart}
              className="bg-green-600/20 hover:bg-green-600/40 text-green-400 border border-green-600/50 p-2 rounded text-xs font-mono transition-colors flex items-center justify-center gap-1"
            >
              <div className="w-1.5 h-1.5 bg-green-500 rounded-full" />
              START
            </button>
            <button
              onClick={handleStop}
              className="bg-red-600/20 hover:bg-red-600/40 text-red-400 border border-red-600/50 p-2 rounded text-xs font-mono transition-colors"
            >
              STOP
            </button>
          </div>
        </div>

        <div className="p-4 border-t border-slate-800 text-[10px] text-slate-600 font-mono flex items-center justify-between">
          <span>SYSTEM STATUS</span>
          <ConnectionStatus />
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 relative bg-neutral-950 overflow-hidden">
        {loading && (
          <div className="absolute inset-0 bg-black/40 backdrop-blur-[1px] z-50 flex items-center justify-center font-mono text-xs text-white/50">
            SYNCING SCHEMA...
          </div>
        )}

        {/* Background Grid */}
        <div
          className="absolute inset-0 opacity-20 pointer-events-none"
          style={{
            backgroundImage: `
              linear-gradient(to right, #334155 1px, transparent 1px),
              linear-gradient(to bottom, #334155 1px, transparent 1px)
            `,
            backgroundSize: '40px 40px'
          }}
        />

        {/* HUD Header */}
        <div className="absolute top-0 left-0 p-6 z-10 w-full pointer-events-none">
          {selectedMachine && (
            <>
              <h1 className="text-3xl font-bold font-mono tracking-widest text-white/90 uppercase flex items-center gap-3 drop-shadow-md">
                {selectedMachine.machineName}
              </h1>
              <div className="flex gap-2 mt-2">
                <div className="inline-flex items-center gap-2 px-2 py-1 bg-cyan-950/30 border border-cyan-900/50 rounded text-cyan-400 text-xs font-mono">
                  <span>ID: {selectedMachine.scene.name}</span>
                  <span className="w-[1px] h-3 bg-cyan-800/50"></span>
                  <span>NODES: {selectedMachine.deviceRegistry.length}</span>
                </div>
                <StepIndicator />
              </div>
            </>
          )}
        </div>

        {/* Zoom Controls (Hidden for R3F OrbitControls) */}
        {/* <div className="absolute bottom-8 right-8 z-50 flex gap-4"> ... </div> */}

        {/* Viewport Center */}
        <div className="absolute inset-0">
          <Canvas shadows orthographic camera={{ position: [1000, -1000, 1000], zoom: 1, near: -10000, far: 20000, up: [0, 0, 1] }}>
            <TelemetryContext.Provider value={telemetryCtx}>
              {/* @ts-ignore */}
              <color attach="background" args={['#101010']} />
              {/* Ambient Light for base visibility */}
              <ambientLight intensity={0.5} />
              {/* Directional Light for definition */}
              <directionalLight position={[1000, -500, 1000]} intensity={1} castShadow />

              <OrbitControls
                makeDefault
                minZoom={0.1}
                maxZoom={10}
                target={[0, 0, 0]}
              />

              <CameraController trigger={resetTrigger} />

              <Stage intensity={0.5} environment="city" adjustCamera={1.2}>
                {selectedMachine && (
                  <DeviceNode3D
                    key={selectedMachine.machineName}
                    node={selectedMachine.scene}
                    registry={selectedMachine.deviceRegistry}
                  />
                )}
              </Stage>
            </TelemetryContext.Provider>
          </Canvas>
        </div>

        {/* View Controls */}
        <div className="absolute bottom-8 right-8 z-50 flex gap-4">
          <div className="flex items-center gap-2 bg-slate-800/90 backdrop-blur p-2 rounded-lg border border-slate-700 shadow-xl">
            <button
              onClick={() => setResetTrigger(t => t + 1)}
              className="h-8 px-3 flex items-center justify-center bg-slate-700 hover:bg-slate-600 rounded text-cyan-400 text-xs font-mono font-bold transition-colors uppercase gap-2"
            >
              <div className="w-3 h-3 border-2 border-current rounded-full relative">
                <div className="absolute inset-0 m-auto w-1 h-1 bg-current rounded-full" />
              </div>
              Reset View
            </button>
          </div>
        </div>

        {/* Vignette Overlay */}
        <div className="absolute inset-0 pointer-events-none bg-[radial-gradient(circle_at_center,transparent_20%,rgba(0,0,0,0.6)_100%)]"></div>
      </div>
    </div >
  );
}

export default function App() {
  return (
    <TelemetryProvider>
      <MachineView />
    </TelemetryProvider>
  );
}
