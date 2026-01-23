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
        private CaptureVisualRegistry _registry = new CaptureVisualRegistry();
        private FlowContext? _context;
        private IVisualFlowInterpreter? _interpreter;
        private readonly List<IDisposable> _subscriptions = new();
        private System.Windows.Forms.Timer? _renderTimer;
        
        // Tracking nodes for highlighting
        private readonly Dictionary<string, SceneNode> _deviceNodeMap = new();

        public KinematicVisualizer(Control entryControl)
        {
            _form = entryControl?.FindForm() ?? throw new InvalidOperationException("KinematicVisualizer requires a control hosted in a Form.");
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
            _registry = new CaptureVisualRegistry();
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
                             else if (style.Type == "Gripper")
                             {
                                 var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                                 sprite.Width = style.Width; sprite.Height = style.Height;
                                 sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                                 sprite.CustomDraw = (g, w, h) => {
                                      float t = (float)Math.Clamp(sprite.CurrentValue / 100.0, 0, 1);
                                      if (style.IsReversed) t = 1 - t;
                                      float openWidth = (float)style.Param1;
                                      float closeWidth = (float)style.Param2;
                                      float gap = (closeWidth + (openWidth - closeWidth) * (1 - t)) / 2f;
                                      float jawLen = w * 0.4f;
                                      float jawThk = h * 0.15f;
                                      using var brush = new SolidBrush(Color.FromArgb(120, 170, 200));
                                      g.FillRectangle(brush, -gap - jawLen, -jawThk/2, jawLen, jawThk);
                                      g.FillRectangle(brush, gap, -jawThk/2, jawLen, jawThk);
                                      using var hub = new SolidBrush(Color.FromArgb(80, 100, 130));
                                      g.FillEllipse(hub, -w/6f, -h/6f, w/3f, h/3f);
                                 };
                                 devNode = sprite;
                             }
                             else if (style.Type == "SuctionPen")
                             {
                                 var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                                 sprite.Width = style.Width; sprite.Height = style.Height;
                                 sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                                 sprite.CustomDraw = (g, w, h) => {
                                      float t = (float)Math.Clamp(sprite.CurrentValue / 100.0, 0, 1);
                                      float r = (float)style.Param1 / 2f;
                                      using var p = new Pen(Color.FromArgb(140, 180, 200), 2);
                                      g.DrawEllipse(p, -r, -r, r*2, r*2);
                                      using var fill = new SolidBrush(Color.FromArgb((int)(60 + 120 * t), 120, 180, 220));
                                      g.FillEllipse(fill, -r+2, -r+2, r*2-4, r*2-4);
                                 };
                                 devNode = sprite;
                             }
                             else 
                             {
                                 if (style.Type == "SlideBlock")
                                 {
                                     var length = (float)(style.Param1 > 0 ? style.Param1 : (style.Width > 0 ? style.Width : 120));
                                     var thickness = style.Height > 0 ? style.Height : 32;
                                     var isVertical = style.IsVertical;

                                     var rail = new SpriteNode { Name = devName + "_Rail" };
                                     rail.PivotX = style.PivotX;
                                     rail.PivotY = style.PivotY;
                                     rail.Width = isVertical ? thickness : length;
                                     rail.Height = isVertical ? length : thickness;
                                     rail.CustomDraw = SpriteDraw.CreateSlideRailDraw(isVertical);

                                     var pad = MathF.Max(2, MathF.Min(rail.Width, rail.Height) * 0.08f);
                                     var railW = (isVertical ? rail.Height : rail.Width) - pad * 2;
                                     var railH = MathF.Max(6, (isVertical ? rail.Width : rail.Height) * 0.22f);
                                     var blockW = MathF.Max(18, MathF.Min(railW * 0.30f, 60));
                                     var travel = MathF.Max(0, railW - blockW);

                                     var output = new StrokeNode
                                     {
                                         Name = devName,
                                         BoundDeviceId = new CylinderID(devName),
                                         IsVertical = isVertical,
                                         IsReversed = style.IsReversed,
                                         Stroke = travel,
                                         BaseX = isVertical ? 0 : -travel / 2f,
                                         BaseY = isVertical ? -travel / 2f : 0
                                     };
 
                                     var carriage = new SpriteNode { Name = devName + "_Carriage" };
                                     carriage.PivotX = 0.5f;
                                     carriage.PivotY = 0.5f;
                                     carriage.Width = blockW;
                                     carriage.Height = MathF.Max(railH * 1.8f, (isVertical ? rail.Width : rail.Height) * 0.55f);
                                     carriage.CustomDraw = SpriteDraw.CreateSlideCarriageDraw();
                                     output.AddChild(carriage);

                                     var slideGroup = new GroupNode { Name = devName + "_Slide" };
                                     slideGroup.AddChild(rail);
                                     slideGroup.AddChild(output);
                                     devNode = slideGroup;
                                 }
                                 else
                                 {
                                     var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                                     sprite.Width = style.Width; sprite.Height = style.Height;
                                     sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;

                                     if (style.Type != "Custom")
                                         sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, style.IsVertical, style.IsReversed);

                                     devNode = sprite;
                                 }
                             }
                         }
                         else
                         {
                             // Default fallback
                             var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName), Width = 60, Height = 24 };
                             sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, isVertical: false, isReversed: false);
                             devNode = sprite;
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
                            string name = n.BoundDeviceId.Name;
                            // Ensure we get the actual simulator instance, even if it's inside a DeviceHub
                            object? dev = (object?)_context.GetDevice<ISimulatorAxis>(name)
                                        ?? _context.GetDevice<ISimulatorCylinder>(name)
                                        ?? _context.GetDevice<ISimulatorVacuum>(name)
                                        ?? _context.GetDevice<object>(name);

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
