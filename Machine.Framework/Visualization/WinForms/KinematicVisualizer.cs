using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Interpreters.Visualization;
using Machine.Framework.Visualization;
using Machine.Framework.Visualization.SceneGraph;
using Machine.Framework.Core.Simulation;
using System.Drawing;
using System.Threading;

namespace Machine.Framework.Visualization.WinForms
{
    public class KinematicVisualizer : IUIVisualizer
    {
        private readonly Form _form;
        private readonly CaptureVisualRegistry _registry = new CaptureVisualRegistry();
        private FlowContext? _context;
        private IVisualFlowInterpreter? _interpreter;
        private readonly List<IDisposable> _subscriptions = new();
        private System.Windows.Forms.Timer? _renderTimer;
        
        // Tracking nodes for highlighting
        private readonly Dictionary<string, SceneNode> _deviceNodeMap = new();

        public KinematicVisualizer(Control entryControl)
        {
            _form = entryControl.FindForm();
        }

        public IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter)
        {
            _interpreter = interpreter;
            return this;
        }

        public IUIVisualizer ObserveContext(FlowContext context)
        {
            _context = context;
            InitializeVisualization();
            return this;
        }

        public IUIVisualizer Visuals(Action<IDeviceVisualRegistry> registryConfig)
        {
            registryConfig(_registry);
            return this;
        }

        public IBindingBuilder Bind(object panel)
        {
            return _registry.Bind(panel);
        }

        public IUIVisualizer AutoHighlight(object panel, DeviceID id)
        {
            return this;
        }

        private void InitializeVisualization()
        {
            if (_context == null) return;
            
            if (_form.InvokeRequired)
            {
                _form.Invoke(new Action(InitializeVisualization));
                return;
            }

            var model = _registry.Model;
            var config = _context.Config; 
            
            if (config == null || config.MountPoints == null) return;

            var builder = new SceneGraphBuilder(config.MountPoints, model);
            
            foreach(var d in _subscriptions) d.Dispose();
            _subscriptions.Clear();
            _renderTimer?.Dispose();
            _deviceNodeMap.Clear();

            var updateList = new List<(SceneNode Node, object Device)>();

            foreach (var binding in model.Bindings)
            {
                if (binding.Panel is not Control panel) continue;

                SceneNode? rootNode = null;

                if (!string.IsNullOrEmpty(binding.TargetRootName))
                {
                    // Case A: Kinematic Mount Binding
                    var rawRoot = builder.Build(binding.TargetRootName!);
                    
                    // Frame wrap for centering/visibility (offset 0,0 to 80,50)
                    var frame = new GroupNode { Name = "Frame_" + binding.TargetRootName };
                    frame.LocalX = 80; 
                    frame.LocalY = 50; 
                    frame.AddChild(rawRoot);
                    
                    rootNode = frame;
                }
                else if (binding.TargetDeviceNames.Count > 0)
                {
                    // Case B: Discrete Device Binding
                    var group = new GroupNode { Name = "AutoRoot" };
                    group.LocalX = 50; group.LocalY = 50; // Padding

                    foreach(var devName in binding.TargetDeviceNames)
                    {
                         SceneNode? devNode = null;
                         if (model.Styles.TryGetValue(devName, out var style))
                         {
                             if (style.Type == "RotaryTable" || style.Type == "LinearGuide" || style.Type == "Default") 
                             {
                                 var axisNode = new AxisNode { Name = devName, BoundDeviceId = new AxisID(devName) };
                                 if (style.Type == "RotaryTable") axisNode.IsRotary = true;
                                 if (style.Type == "LinearGuide") 
                                 { 
                                     axisNode.Length = (float)style.Param1; 
                                     axisNode.Width = (float)style.Param2; 
                                     axisNode.Height = (float)(style.Height > 0 ? style.Height : style.Width); 
                                 }
                                 axisNode.IsVertical = style.IsVertical;
                                 devNode = axisNode;
                             }
                             else 
                             {
                                 var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                                 sprite.Width = style.Width; sprite.Height = style.Height;
                                 sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                                 if (style.Type == "SlideBlock") sprite.Color = System.Drawing.Color.SteelBlue;
                                 devNode = sprite;
                             }
                         }
                         else
                         {
                             // Default fallback
                             devNode = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName), Width=20, Height=20 };
                         }

                         if (devNode != null)
                         {
                             // Stack horizontally
                             devNode.LocalX = group.Children.Count * 120;
                             group.AddChild(devNode);
                         }
                    }
                    rootNode = group;
                }

                if (rootNode != null)
                {
                    RegisterNodesCached(rootNode); 

                    // Setup Canvas
                    panel.Controls.Clear();
                    var canvas = new SceneGraphCanvas { Dock = DockStyle.Fill };
                    canvas.SetRoot(rootNode);
                    panel.Controls.Add(canvas);

                    // Collect update targets
                    void CollectUpdates(SceneNode n)
                    {
                        if (n.BoundDeviceId != null)
                        {
                            var dev = _context.GetDevice<object>(n.BoundDeviceId.Name); 
                            if (dev != null) updateList.Add((n, dev));
                        }
                        foreach(var c in n.Children) CollectUpdates(c);
                    }
                    CollectUpdates(rootNode);
                    
                    _subscriptions.Add(System.Reactive.Disposables.Disposable.Create(() => canvas.Dispose()));
                }
            }

            // Highlighting Subscription
            if (_interpreter != null)
            {
               var sync = SynchronizationContext.Current;
               if (sync != null)
               {
                   _subscriptions.Add(
                       _interpreter.TraceStream
                       .ObserveOn(sync)
                       .Subscribe(evt => 
                       {
                           if (_deviceNodeMap.TryGetValue(evt.TargetDevice, out var node))
                           {
                               if (evt.Status == StepStatus.Running)
                                   node.HighlightColor = System.Drawing.Color.LimeGreen;
                               else if (evt.Status == StepStatus.Error)
                                   node.HighlightColor = System.Drawing.Color.Red;
                               else
                                   node.HighlightColor = null; // Clear
                               
                               if (evt.Status == StepStatus.Completed)
                                   node.HighlightColor = null; 
                           }
                       })
                   );
               }
            }

            // Start Render Loop
            _renderTimer = new System.Windows.Forms.Timer { Interval = 30 };
            _renderTimer.Tick += (s, e) => 
            {
                // Update Nodes
                foreach(var item in updateList) UpdateNodeFromDevice(item.Node, item.Device);
                
                // Redraw
                foreach(var binding in model.Bindings)
                {
                    if (binding.Panel is Control p && p.Controls.Count > 0 && p.Controls[0] is SceneGraphCanvas c)
                        c.Invalidate();
                }
            };
            _renderTimer.Start();
        }

        private void RegisterNodesCached(SceneNode node)
        {
            if (node.BoundDeviceId != null)
            {
                _deviceNodeMap[node.BoundDeviceId.Name] = node;
            }
            foreach (var child in node.Children) RegisterNodesCached(child);
        }

        private void UpdateNodeFromDevice(SceneNode node, object device)
        {
            try
            {
                if (device is ISimulatorAxis axis)
                {
                    node.Update(axis.CurrentState.Position);
                }
                else if (device is ISimulatorCylinder cyl)
                {
                    node.Update(cyl.CurrentState.Position * 100.0);
                }
                else if (device is ISimulatorVacuum vac)
                {
                     node.Update(vac.CurrentState.IsOn ? 1.0 : 0.0);
                }
                else
                {
                    // Fallback
                    dynamic d = device;
                    double val = 0;
                    try { val = d.Position; } 
                    catch 
                    {
                         try { val = d.IsOut ? 100 : 0; } catch { val = 0; }
                    }
                    node.Update(val);
                }
            }
            catch { /* Ignore */ }
        }
    }
}
