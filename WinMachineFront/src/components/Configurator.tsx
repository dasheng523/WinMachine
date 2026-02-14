import React, { useState } from 'react';
import { 
  Plus, 
  Trash2, 
  Settings2, 
  Box, 
  RotateCw, 
  GripHorizontal, 
  ArrowDownToLine, 
  LayoutGrid,
  Save,
  Hammer,
  Play,
  ChevronDown,
  ChevronRight
} from 'lucide-react';
import type { SceneNode, Transform, Device, DeviceMeta } from '../types';
import { Canvas } from '@react-three/fiber';
import { Stage, OrbitControls, Grid } from '@react-three/drei';
import { DeviceNode3D } from './DeviceNode3D';

interface PartTemplate {
  type: SceneNode['nodeType'];
  name: string;
  icon: React.ReactNode;
  description: string;
  defaultStroke?: Transform;
  defaultMeta?: DeviceMeta;
}

const PART_TEMPLATES: PartTemplate[] = [
  { 
    type: 'LinearGuide', 
    name: '直线导轨', 
    icon: <ArrowDownToLine size={20} />, 
    description: '提供直线运动的基础结构',
    defaultMeta: { length: 200, width: 32 }
  },
  { 
    type: 'RotaryTable', 
    name: '分度转盘', 
    icon: <RotateCw size={20} />, 
    description: '支持旋转角度定位',
    defaultMeta: { radius: 40 }
  },
  { 
    type: 'Gripper', 
    name: '气动夹爪', 
    icon: <GripHorizontal size={20} />, 
    description: '用于抓取和释放零件',
    defaultMeta: { width: 32, height: 32, openWidth: 40, closeWidth: 10 }
  },
  { 
    type: 'SuctionPen', 
    name: '真空吸笔', 
    icon: <Plus size={20} />, 
    description: '通过负压吸取轻小零件',
    defaultMeta: { diameter: 10 }
  },
  { 
    type: 'SlideBlock', 
    name: '滑块组件', 
    icon: <Box size={20} />, 
    description: '安装在导轨上的移动单元',
    defaultMeta: { size: 40 }
  },
  { 
    type: 'Group', 
    name: '组件容器', 
    icon: <LayoutGrid size={20} />, 
    description: '用于对多个零件进行逻辑分组' 
  },
];

export const Configurator: React.FC<{ onExit?: () => void }> = ({ onExit }) => {
  const [assembly, setAssembly] = useState<SceneNode[]>([]);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [registry, setRegistry] = useState<Device[]>([]);
  const [draggedNodeName, setDraggedNodeName] = useState<string | null>(null);

  const findNodeAndParent = (nodes: SceneNode[], targetName: string, parent: SceneNode | null = null): { node: SceneNode, parent: SceneNode | null } | null => {
    for (const node of nodes) {
      if (node.name === targetName) return { node, parent };
      if (node.children) {
        const result = findNodeAndParent(node.children, targetName, node);
        if (result) return result;
      }
    }
    return null;
  };

  const handleAddPart = (template: PartTemplate, parentName: string | null = null) => {
    const deviceId = `dev_${Math.random().toString(36).substr(2, 4)}`;
    const newNode: SceneNode = {
      name: `${template.name}_${Math.random().toString(36).substr(2, 4)}`,
      nodeType: template.type,
      linkedDeviceId: deviceId,
      offset: { x: 0, y: 0, z: 0 },
      rotation: { x: 0, y: 0, z: 0 },
      children: []
    };

    const newDevice: Device = {
      id: deviceId,
      type: template.type,
      baseType: template.type,
      meta: template.defaultMeta || {}
    };

    setRegistry([...registry, newDevice]);

    if (!parentName) {
      setAssembly([...assembly, newNode]);
    } else {
      const updateChildren = (nodes: SceneNode[]): SceneNode[] => {
        return nodes.map(node => {
          if (node.name === parentName) {
            return { ...node, children: [...(node.children || []), newNode] };
          }
          if (node.children) {
            return { ...node, children: updateChildren(node.children) };
          }
          return node;
        });
      };
      setAssembly(updateChildren(assembly));
    }
    setSelectedNodeId(newNode.name);
  };

  const handleRemovePart = (name: string) => {
    const result = findNodeAndParent(assembly, name);
    if (!result) return;

    const { node: nodeToRemove, parent } = result;

    // Remove from registry recursively
    const unregisterDevices = (n: SceneNode) => {
      if (n.linkedDeviceId) {
        setRegistry(prev => prev.filter(d => d.id !== n.linkedDeviceId));
      }
      n.children?.forEach(unregisterDevices);
    };
    unregisterDevices(nodeToRemove);

    if (!parent) {
      setAssembly(assembly.filter(n => n.name !== name));
    } else {
      const updateChildren = (nodes: SceneNode[]): SceneNode[] => {
        return nodes.map(node => {
          if (node.name === parent.name) {
            return { ...node, children: (node.children || []).filter(c => c.name !== name) };
          }
          if (node.children) {
            return { ...node, children: updateChildren(node.children) };
          }
          return node;
        });
      };
      setAssembly(updateChildren(assembly));
    }

    if (selectedNodeId === name) setSelectedNodeId(null);
  };

  const updateNodeProperty = (name: string, property: 'offset' | 'rotation', axis: 'x' | 'y' | 'z', value: number) => {
    const updateNodes = (nodes: SceneNode[]): SceneNode[] => {
      return nodes.map(node => {
        if (node.name === name) {
          const currentVal = node[property] || { x: 0, y: 0, z: 0 };
          return {
            ...node,
            [property]: {
              ...currentVal,
              [axis]: value
            }
          };
        }
        if (node.children) {
          return { ...node, children: updateNodes(node.children) };
        }
        return node;
      });
    };
    setAssembly(updateNodes(assembly));
  };

  const moveNode = (nodeName: string, targetParentName: string | null) => {
    if (nodeName === targetParentName) return;

    const result = findNodeAndParent(assembly, nodeName);
    if (!result) return;
    const { node: nodeToMove } = result;

    // Prevent moving a parent into its own child
    const isChildOf = (parent: SceneNode, targetName: string): boolean => {
      if (parent.children?.some(c => c.name === targetName)) return true;
      return parent.children?.some(c => isChildOf(c, targetName)) || false;
    };
    if (targetParentName && isChildOf(nodeToMove, targetParentName)) return;

    // 1. Remove from current position
    let newAssembly = assembly;
    const removeNode = (nodes: SceneNode[]): SceneNode[] => {
      return nodes.filter(n => n.name !== nodeName).map(n => ({
        ...n,
        children: n.children ? removeNode(n.children) : []
      }));
    };
    newAssembly = removeNode(newAssembly);

    // 2. Add to new position
    if (targetParentName === null) {
      newAssembly = [...newAssembly, nodeToMove];
    } else {
      const addToParent = (nodes: SceneNode[]): SceneNode[] => {
        return nodes.map(n => {
          if (n.name === targetParentName) {
            return { ...n, children: [...(n.children || []), nodeToMove] };
          }
          return { ...n, children: n.children ? addToParent(n.children) : [] };
        });
      };
      newAssembly = addToParent(newAssembly);
    }

    setAssembly(newAssembly);
  };

  const [inputValues, setInputValues] = useState<Record<string, string>>({});

  const handleInputChange = (nodeName: string, property: 'offset' | 'rotation', axis: 'x' | 'y' | 'z', valueStr: string) => {
    const key = `${nodeName}-${property}-${axis}`;
    setInputValues({ ...inputValues, [key]: valueStr });

    const val = parseFloat(valueStr);
    if (!isNaN(val)) {
      updateNodeProperty(nodeName, property, axis, val);
    }
  };

  const getInputValue = (nodeName: string, property: 'offset' | 'rotation', axis: 'x' | 'y' | 'z') => {
    const key = `${nodeName}-${property}-${axis}`;
    if (inputValues[key] !== undefined) return inputValues[key];
    
    const node = findNodeAndParent(assembly, nodeName)?.node;
    return node?.[property]?.[axis]?.toString() || '0';
  };

  const [dragOverNodeName, setDragOverNodeName] = useState<string | null>(null);

  const HierarchyTreeItem: React.FC<{ node: SceneNode; level: number }> = ({ node, level }) => {
    const isExpanded = true; // Could add state for this
    const isSelected = selectedNodeId === node.name;
    const isDragOver = dragOverNodeName === node.name;
    const template = PART_TEMPLATES.find(t => t.type === node.nodeType);

    return (
      <div className="space-y-0.5">
        <div 
          draggable
          onDragStart={(e) => {
            e.dataTransfer.setData('nodeName', node.name);
            setDraggedNodeName(node.name);
          }}
          onDragEnd={() => {
            setDraggedNodeName(null);
            setDragOverNodeName(null);
          }}
          onDragOver={(e) => {
            if (node.nodeType === 'Group' && draggedNodeName !== node.name) {
              e.preventDefault();
              e.stopPropagation();
              if (dragOverNodeName !== node.name) setDragOverNodeName(node.name);
            }
          }}
          onDragLeave={() => {
            if (dragOverNodeName === node.name) setDragOverNodeName(null);
          }}
          onDrop={(e) => {
            setDragOverNodeName(null);
            const droppedNodeName = e.dataTransfer.getData('nodeName');
            const partType = e.dataTransfer.getData('partType');
            
            if (droppedNodeName) {
              e.stopPropagation();
              moveNode(droppedNodeName, node.name);
            } else if (partType) {
              e.stopPropagation();
              const template = PART_TEMPLATES.find(t => t.type === partType);
              if (template) handleAddPart(template, node.name);
            }
          }}
          onClick={() => setSelectedNodeId(node.name)}
          className={`group flex items-center justify-between p-2 rounded cursor-grab active:cursor-grabbing transition-all border select-none ${
            isSelected 
              ? 'bg-cyan-500/10 border-cyan-500/50 text-cyan-400 shadow-[inset_0_0_10px_rgba(6,182,212,0.1)]' 
              : isDragOver
                ? 'bg-amber-500/20 border-amber-500/50 text-amber-400'
                : 'bg-transparent border-transparent hover:bg-slate-800/50 text-slate-400'
          }`}
          style={{ marginLeft: level * 12 }}
        >
          <div className="flex items-center gap-2 truncate pointer-events-none">
            <div className="opacity-70">
              {node.children && node.children.length > 0 ? (
                isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />
              ) : (
                <div className="w-3.5" />
              )}
            </div>
            <div className="opacity-70">
              {template?.icon}
            </div>
            <span className="text-xs font-mono truncate">{node.name}</span>
          </div>
          <button 
            onClick={(e) => { e.stopPropagation(); handleRemovePart(node.name); }}
            className="opacity-0 group-hover:opacity-100 p-1 hover:text-red-400 transition-opacity pointer-events-auto"
          >
            <Trash2 size={12} />
          </button>
        </div>
        {node.children && node.children.length > 0 && (
          <div className="animate-in slide-in-from-left-1">
            {node.children.map(child => (
              <HierarchyTreeItem key={child.name} node={child} level={level + 1} />
            ))}
          </div>
        )}
      </div>
    );
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    const templateType = e.dataTransfer.getData('partType');
    const template = PART_TEMPLATES.find(t => t.type === templateType);
    if (template) {
      handleAddPart(template);
    }
  };

  return (
    <div className="flex w-full h-full bg-neutral-900 text-slate-200 overflow-hidden font-sans">
      {/* Left Sidebar: Hierarchy */}
      <div className="w-64 bg-slate-900 border-r border-slate-700 flex flex-col z-20 shadow-2xl">
        <div className="p-4 border-b border-slate-700 bg-slate-900/50 backdrop-blur-sm flex justify-between items-center">
          <div>
            <h2 className="text-sm font-bold font-mono text-cyan-500 tracking-wider">ASSEMBLY TREE</h2>
            <p className="text-[10px] text-slate-500 font-mono uppercase">Hierarchy view</p>
          </div>
          <Hammer size={16} className="text-cyan-500 opacity-50" />
        </div>
        
        <div className="flex-1 overflow-y-auto p-2 space-y-1">
          {assembly.length === 0 ? (
            <div className="h-full flex flex-col items-center justify-center opacity-30 text-xs text-center p-4">
              <Box size={32} className="mb-2" />
              <p>No parts added yet. Drag from the right library.</p>
            </div>
          ) : (
            assembly.map(node => (
              <HierarchyTreeItem key={node.name} node={node} level={0} />
            ))
          )}
        </div>

        {/* Property Editor */}
        {selectedNodeId && (
          <div className="p-4 border-t border-slate-800 bg-slate-900/50 animate-in slide-in-from-bottom-2">
            <div className="flex items-center gap-2 mb-3 text-amber-500">
              <Settings2 size={14} />
              <h3 className="text-[10px] font-bold font-mono uppercase tracking-tighter">Properties</h3>
            </div>
            <div className="space-y-4">
              <div>
                <label className="text-[8px] text-slate-500 font-mono uppercase mb-1 block">Component Name</label>
                <input 
                  type="text" 
                  value={selectedNodeId}
                  onChange={(e) => {
                    const newName = e.target.value;
                    const updateNodes = (nodes: SceneNode[]): SceneNode[] => {
                      return nodes.map(n => {
                        if (n.name === selectedNodeId) return { ...n, name: newName };
                        return { ...n, children: n.children ? updateNodes(n.children) : [] };
                      });
                    };
                    setAssembly(updateNodes(assembly));
                    setSelectedNodeId(newName);
                  }}
                  className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-xs font-mono text-slate-200 focus:border-cyan-500 outline-none"
                />
              </div>
              
              <div className="space-y-2">
                <label className="text-[8px] text-slate-500 font-mono uppercase block">Position Offset (mm)</label>
                <div className="grid grid-cols-3 gap-1">
                  {(['x', 'y', 'z'] as const).map(axis => (
                    <div key={axis}>
                      <div className="flex justify-between items-center mb-0.5">
                        <span className="text-[7px] text-slate-600 font-mono uppercase">{axis}</span>
                      </div>
                      <input 
                        type="text" 
                        value={getInputValue(selectedNodeId, 'offset', axis)}
                        onChange={(e) => handleInputChange(selectedNodeId, 'offset', axis, e.target.value)}
                        className="w-full bg-slate-800 border border-slate-700 rounded px-1 py-1 text-[10px] font-mono text-slate-200 outline-none focus:border-cyan-500/50"
                      />
                    </div>
                  ))}
                </div>
              </div>

              <div className="space-y-2">
                <label className="text-[8px] text-slate-500 font-mono uppercase block">Rotation (deg)</label>
                <div className="grid grid-cols-3 gap-1">
                  {(['x', 'y', 'z'] as const).map(axis => (
                    <div key={axis}>
                      <div className="flex justify-between items-center mb-0.5">
                        <span className="text-[7px] text-slate-600 font-mono uppercase">{axis}</span>
                      </div>
                      <input 
                        type="text" 
                        value={getInputValue(selectedNodeId, 'rotation', axis)}
                        onChange={(e) => handleInputChange(selectedNodeId, 'rotation', axis, e.target.value)}
                        className="w-full bg-slate-800 border border-slate-700 rounded px-1 py-1 text-[10px] font-mono text-slate-200 outline-none focus:border-cyan-500/50"
                      />
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        <div className="p-4 border-t border-slate-800 bg-slate-900/80">
          <button className="w-full bg-cyan-600 hover:bg-cyan-500 text-white p-2 rounded text-xs font-mono font-bold transition-all flex items-center justify-center gap-2 shadow-lg shadow-cyan-900/20">
            <Save size={14} />
            SAVE CONFIG
          </button>
        </div>
      </div>

      {/* Main Content Area: Assembly Canvas */}
      <div className="flex-1 relative bg-neutral-950 overflow-hidden flex flex-col">
        {/* Background Grid */}
        <div 
          className="absolute inset-0 opacity-10 pointer-events-none"
          style={{ 
            backgroundImage: `
              linear-gradient(to right, #334155 1px, transparent 1px),
              linear-gradient(to bottom, #334155 1px, transparent 1px)
            `,
            backgroundSize: '40px 40px'
          }}
        />

        {/* HUD Overlay */}
        <div className="absolute top-6 left-6 z-10 w-full pr-12 pointer-events-none flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold font-mono tracking-widest text-white/90 uppercase flex items-center gap-3">
              CONFIGURATION MODE
            </h1>
            <div className="flex gap-2 mt-2">
              <div className="inline-flex items-center gap-2 px-2 py-1 bg-amber-950/30 border border-amber-900/50 rounded text-amber-400 text-xs font-mono">
                <Settings2 size={12} />
                <span>DRAFT: NEW_MACHINE_01</span>
              </div>
            </div>
          </div>
          
          <button 
            onClick={onExit}
            className="pointer-events-auto flex items-center gap-2 px-4 py-2 bg-slate-800 hover:bg-slate-700 border border-slate-700 rounded-lg text-slate-300 hover:text-white transition-all font-mono text-xs font-bold shadow-xl"
          >
            <Play size={14} className="text-cyan-500" />
            EXIT TO OPERATION
          </button>
        </div>

        {/* Assembly Area: 3D Canvas */}
        <div className="flex-1 relative flex flex-col">
          <div 
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            className="flex-1 relative bg-neutral-950"
          >
            {assembly.length === 0 && (
              <div className="absolute inset-0 z-10 flex items-center justify-center pointer-events-none">
                <div className="text-slate-600 font-mono text-center">
                  <LayoutGrid size={48} className="mx-auto mb-4 opacity-20" />
                  <p className="text-sm">DROP COMPONENTS HERE TO START BUILDING</p>
                </div>
              </div>
            )}
            
            <Canvas shadows orthographic camera={{ position: [1000, -1000, 1000], zoom: 1.5, near: -10000, far: 20000, up: [0, 0, 1] }}>
              <color attach="background" args={['#0a0a0a']} />
              <ambientLight intensity={0.5} />
              <directionalLight position={[1000, -500, 1000]} intensity={1} castShadow />
              <OrbitControls makeDefault target={[0, 0, 0]} />
              
              <Grid 
                infiniteGrid 
                fadeDistance={1000} 
                fadeStrength={5} 
                cellSize={40} 
                sectionSize={200} 
                sectionColor="#334155" 
                cellColor="#1e293b" 
                rotation={[Math.PI / 2, 0, 0]} 
              />

              <Stage intensity={0.5} environment="city" adjustCamera={false}>
                 {assembly.map((node, idx) => (
                   <DeviceNode3D 
                     key={`${node.name}-${idx}`} 
                     node={node} 
                     registry={registry} 
                     isSelected={selectedNodeId === node.name}
                   />
                 ))}
               </Stage>
            </Canvas>
          </div>
        </div>
      </div>

      {/* Right Sidebar: Parts Library */}
      <div className="w-80 bg-slate-900 border-l border-slate-700 flex flex-col z-20 shadow-2xl">
        <div className="p-4 border-b border-slate-700 bg-slate-900/50 backdrop-blur-sm">
          <h2 className="text-sm font-bold font-mono text-cyan-500 tracking-wider">PARTS LIBRARY</h2>
          <p className="text-[10px] text-slate-500 font-mono uppercase">Drag to add components</p>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-4">
          {PART_TEMPLATES.map((template) => (
            <div 
              key={template.type}
              draggable
              onDragStart={(e) => {
                e.dataTransfer.setData('partType', template.type);
                e.dataTransfer.effectAllowed = 'copy';
              }}
              onClick={() => handleAddPart(template)}
              className="bg-slate-800/50 border border-slate-700 rounded-lg p-3 cursor-grab active:cursor-grabbing hover:border-cyan-500/50 hover:bg-slate-800 transition-all group"
            >
              <div className="flex items-center gap-3 mb-2">
                <div className="p-2 bg-slate-900 rounded-md text-cyan-400 group-hover:scale-110 transition-transform">
                  {template.icon}
                </div>
                <div>
                  <h3 className="text-xs font-bold text-slate-200 font-mono">{template.name}</h3>
                  <p className="text-[10px] text-slate-500 font-mono uppercase">{template.type}</p>
                </div>
              </div>
              <p className="text-[10px] text-slate-400 leading-relaxed">
                {template.description}
              </p>
            </div>
          ))}
        </div>

        <div className="p-4 border-t border-slate-800 bg-slate-900/80">
          <div className="bg-slate-800/50 border border-dashed border-slate-700 rounded p-3 text-center">
            <p className="text-[10px] text-slate-500 font-mono italic">
              * Click or drag components to add them to the assembly area.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
