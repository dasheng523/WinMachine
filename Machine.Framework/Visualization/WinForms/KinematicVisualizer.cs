using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Primitives;

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

        public IBindingBuilder Bind(object panel) => _registry.Bind(panel);
        public IUIVisualizer AutoHighlight(object panel, DeviceID id) => this;

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
                    var rawRoot = builder.Build(binding.TargetRootName!);
                    var frame = new GroupNode { Name = "Frame_" + binding.TargetRootName };
                    frame.LocalX = 80; frame.LocalY = 50; 
                    frame.AddChild(rawRoot);
                    rootNode = frame;
                }
                else if (binding.TargetDeviceNames.Count > 0)
                {
                    var group = new GroupNode { Name = "AutoRoot" };
                    group.LocalX = 50; group.LocalY = 50; 

                    foreach(var devName in binding.TargetDeviceNames)
                    {
                         SceneNode? devNode = null;
                         if (model.Styles.TryGetValue(devName, out var style))
                         {
                             if (style.Type == "LinearGuide" || style.Type == "Default") 
                             {
                                 var axisNode = new AxisNode { Name = devName, BoundDeviceId = new AxisID(devName) };
                                 axisNode.IsVertical = style.IsVertical;

                                 if (style.Type == "LinearGuide") 
                                 { 
                                     axisNode.Length = (float)style.Param1; 
                                     axisNode.Width = (float)(style.Param2 > 0 ? style.Param2 : 40); 
                                     axisNode.Height = (float)(style.Height > 0 ? style.Height : axisNode.Width); 

                                     // Retrieve travel range 
                                     double min = 0, max = axisNode.Length > 0 ? axisNode.Length : 200;
                                     if (config.AxisConfigs.TryGetValue(devName, out var ax))
                                     {
                                         min = ax.SoftLimits?.Min ?? 0;
                                         max = ax.SoftLimits?.Max ?? (axisNode.Length > 0 ? axisNode.Length : 200);
                                     }
                                     
                                     // 设置行程范围以正确映射位置到视觉坐标
                                     axisNode.TravelMin = (float)min;
                                     axisNode.TravelMax = (float)max;
 
                                     var rail = new SpriteNode { Name = devName + "_Rail" };
                                     rail.PivotX = 0.5f; rail.PivotY = 0.5f;
                                     float rLen = (float)(max - min);
                                     float rWid = axisNode.Width * 0.6f;
                                     rail.Width = axisNode.IsVertical ? rWid : rLen;
                                     rail.Height = axisNode.IsVertical ? rLen : rWid;
                                     
                                     if (axisNode.IsVertical) rail.LocalY = rLen / 2f + (float)min;
                                     else rail.LocalX = rLen / 2f + (float)min;
 
                                     rail.CustomDraw = SpriteDraw.CreateMotorRailDraw(axisNode.IsVertical, min, max);
 
                                     var slideGroup = new GroupNode { Name = devName + "_LinearGroup" };
                                     slideGroup.AddChild(rail);
                                     slideGroup.AddChild(axisNode);
                                     devNode = slideGroup;
                                 }
                                 else {
                                     devNode = axisNode;
                                 }
                             }
                             else if (style.Type == "RotaryTable")
                             {
                                 var axisNode = new AxisNode { Name = devName, BoundDeviceId = new AxisID(devName) };
                                 axisNode.IsRotary = true;
                                 axisNode.Width = (float)(style.Param1 > 0 ? style.Param1 : 40);
                                 devNode = axisNode;
                             }
                             else if (style.Type == "Gripper")
                             {
                                 var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                                 sprite.Width = style.Width; sprite.Height = style.Height;
                                 sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                                 sprite.CustomDraw = SpriteDraw.CreateGripperDraw(() => sprite.CurrentValue, style.IsReversed);
                                 devNode = sprite;
                             }
                             else if (style.Type == "SuctionPen")
                             {
                                 var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName) };
                                 sprite.Width = style.Width; sprite.Height = style.Height;
                                 sprite.PivotX = style.PivotX; sprite.PivotY = style.PivotY;
                                 sprite.CustomDraw = SpriteDraw.CreateSuctionPenDraw(() => sprite.CurrentValue);
                                 devNode = sprite;
                             }
                             else if (style.Type == "SlideBlock")
                             {
                                 var length = (float)(style.Param1 > 0 ? style.Param1 : (style.Width > 0 ? style.Width : 120));
                                 var thickness = style.Height > 0 ? style.Height : 32;
                                 var isVertical = style.IsVertical;

                                 var rail = new SpriteNode { Name = devName + "_Rail" };
                                 rail.PivotX = 0; rail.PivotY = 0.5f;
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
                                     Name = devName, BoundDeviceId = new CylinderID(devName),
                                     IsVertical = isVertical, IsReversed = style.IsReversed,
                                     Stroke = travel, BaseX = 0, BaseY = 0
                                 };

                                 var carriage = new SpriteNode { Name = devName + "_Carriage" };
                                 carriage.PivotX = 0.5f; carriage.PivotY = 0.5f;
                                 carriage.Width = blockW; carriage.Height = MathF.Max(railH * 1.8f, (isVertical ? rail.Width : rail.Height) * 0.55f);
                                 carriage.CustomDraw = SpriteDraw.CreateSlideCarriageDraw();
                                 output.AddChild(carriage);

                                 var slideGroup = new GroupNode { Name = devName + "_Slide" };
                                 slideGroup.AddChild(rail); slideGroup.AddChild(output);
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
                         else
                         {
                             var sprite = new SpriteNode { Name = devName, BoundDeviceId = new CylinderID(devName), Width = 60, Height = 24 };
                             sprite.CustomDraw = SpriteDraw.CreateDefaultCylinderDraw(() => sprite.CurrentValue, isVertical: false, isReversed: false);
                             devNode = sprite;
                         }

                         if (devNode != null)
                         {
                             devNode.LocalX = group.Children.Count * 120;
                             group.AddChild(devNode);
                         }
                    }
                    rootNode = group;
                }

                if (rootNode != null)
                {
                    RegisterNodesCached(rootNode); 
                    panel.Controls.Clear();
                    var canvas = new SceneGraphCanvas { Dock = DockStyle.Fill };
                    canvas.SetRoot(rootNode);
                    panel.Controls.Add(canvas);

                    void CollectUpdates(SceneNode n)
                    {
                        if (n.BoundDeviceId != null)
                        {
                            string name = n.BoundDeviceId.Name;
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

            if (_interpreter != null)
            {
               var sync = SynchronizationContext.Current;
               if (sync != null)
               {
                   _subscriptions.Add(_interpreter.TraceStream.ObserveOn(sync).Subscribe(evt => 
                   {
                       // We no longer draw ugly borders on devices. 
                       // The realistic animation itself provides the active feedback.
                   }));
               }
            }

            _renderTimer = new System.Windows.Forms.Timer { Interval = 30 };
            _renderTimer.Tick += (s, e) => 
            {
                foreach(var item in updateList) UpdateNodeFromDevice(item.Node, item.Device);
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
            if (node.BoundDeviceId != null) _deviceNodeMap[node.BoundDeviceId.Name] = node;
            foreach (var child in node.Children) RegisterNodesCached(child);
        }

        private void UpdateNodeFromDevice(SceneNode node, object device)
        {
            try
            {
                if (device is ISimulatorAxis axis) node.Update(axis.CurrentState.Position);
                else if (device is ISimulatorCylinder cyl) node.Update(cyl.CurrentState.Position * 100.0);
                else if (device is ISimulatorVacuum vac) node.Update(vac.CurrentState.IsOn ? 1.0 : 0.0);
                else {
                    dynamic d = device; double val = 0;
                    try { val = d.Position; } catch { try { val = d.IsOut ? 100 : 0; } catch { } }
                    node.Update(val);
                }
            }
            catch { }
        }
    }
}