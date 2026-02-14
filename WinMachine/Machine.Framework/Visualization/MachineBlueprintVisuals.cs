using System;
using System.Collections.Generic;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Flow;
using Machine.Framework.Visualization.SceneGraph;

namespace Machine.Framework.Visualization
{
    // ... [Interfaces defined previously: IVisualFlowInterpreter, IUIVisualizer, IDeviceVisualRegistry, IBindingBuilder, etc.] remain the same ...
    // To save context, I will only include the NEW Definitions and modifications to Stub.

    public interface IVisualFlowInterpreter : IFlowInterpreter { IObservable<ActiveStepUpdate> TraceStream { get; } }

    public interface IUIVisualizer
    {
        IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter);
        IUIVisualizer ObserveContext(FlowContext context);
        IUIVisualizer Visuals(Action<IDeviceVisualRegistry> registryConfig);
        IBindingBuilder Bind(object panel);
        IUIVisualizer AutoHighlight(object panel, DeviceID id);
    }

    public interface IDeviceVisualRegistry
    {
        IAxisVisualBuilder For(AxisID axis);
        ICylinderVisualBuilder For(CylinderID cylinder);
        IBindingBuilder Bind(object panel);
        IDeviceVisualRegistry AutoHighlight(object panel, DeviceID id);
    }

    public interface IBindingBuilder
    {
        IBindingBuilder ToAxis(AxisID axis);
        IBindingBuilder ToCylinder(CylinderID cylinder);
        IBindingBuilder TargetRoot(string rootName);
        IBindingBuilder Vertical();
        IBindingBuilder Horizontal();
        IBindingBuilder Map(Func<double, object> mapper);
    }

    public interface IAxisVisualBuilder
    {
        IAxisVisualBuilder AsLinearGuide(double length, double sliderWidth);
        IAxisVisualBuilder AsRotaryTable(double radius);
        IAxisVisualBuilder AsCustom(string modelPath);
        IAxisVisualBuilder WithPivot(double x, double y);
        IAxisVisualBuilder WithSize(double width, double height);
        IAxisVisualBuilder Horizontal();
        IAxisVisualBuilder Vertical();
        IAxisVisualBuilder Reversed();
        IAxisVisualBuilder Forward();
    }

    public interface ICylinderVisualBuilder
    {
        ICylinderVisualBuilder AsSlideBlock(double? size = null);
        ICylinderVisualBuilder AsGripper(double open, double close);
        ICylinderVisualBuilder AsSuctionPen(double diameter);
        ICylinderVisualBuilder AsCustom(string modelPath);
        ICylinderVisualBuilder WithPivot(double x, double y);
        ICylinderVisualBuilder WithSize(double width, double height);
        ICylinderVisualBuilder Vertical();
        ICylinderVisualBuilder Horizontal();
        ICylinderVisualBuilder Reversed();
        ICylinderVisualBuilder Forward();
    }

    public enum StepStatus { Ready, Running, Completed, Error }
    public record ActiveStepUpdate(string TargetDevice, string Name, StepStatus Status);

    // --- Data Models for Style Storage ---
    public class VisualDefinitionModel
    {
        public Dictionary<string, VisualStyleDef> Styles { get; } = new();
        public List<VisualBindingDef> Bindings { get; } = new();
    }

    public class VisualStyleDef
    {
        public string DeviceName { get; set; } = "";
        public string Type { get; set; } = "Default"; // LinearGuide, RotaryTable, Gripper...
        public float Width { get; set; } = 32;
        public float Height { get; set; } = 32;
        public float PivotX { get; set; } = 0.5f;
        public float PivotY { get; set; } = 0.5f;
        public bool IsVertical { get; set; }
        public bool IsReversed { get; set; }
        // specific params
        public double Param1 { get; set; } 
        public double Param2 { get; set; }
    }

    public class VisualBindingDef
    {
        public object Panel { get; set; }
        public List<string> TargetDeviceNames { get; set; } = new();
        public string? TargetRootName { get; set; }
    }

    // --- Implementation of Registry to capture data ---
    public class CaptureVisualRegistry : IDeviceVisualRegistry
    {
        public VisualDefinitionModel Model { get; } = new();

        public IAxisVisualBuilder For(AxisID axis) => new CaptureAxisBuilder(GetOrCreate(axis.Name));
        public ICylinderVisualBuilder For(CylinderID cylinder) => new CaptureCylinderBuilder(GetOrCreate(cylinder.Name));
        
        public IBindingBuilder Bind(object panel) 
        {
            var def = new VisualBindingDef { Panel = panel };
            Model.Bindings.Add(def);
            return new CaptureBindingBuilder(def);
        }

        public IDeviceVisualRegistry AutoHighlight(object panel, DeviceID id) => this; // Ignore for model

        private VisualStyleDef GetOrCreate(string name)
        {
            if (!Model.Styles.TryGetValue(name, out var style))
            {
                style = new VisualStyleDef { DeviceName = name };
                Model.Styles[name] = style;
            }
            return style;
        }
    }

    internal class CaptureAxisBuilder : IAxisVisualBuilder
    {
        private readonly VisualStyleDef _def;
        public CaptureAxisBuilder(VisualStyleDef def) => _def = def;

        public IAxisVisualBuilder AsLinearGuide(double l, double s) { _def.Type = "LinearGuide"; _def.Param1 = l; _def.Param2 = s; return this; }
        public IAxisVisualBuilder AsRotaryTable(double r) { _def.Type = "RotaryTable"; _def.Param1 = r; return this; }
        public IAxisVisualBuilder AsCustom(string p) { _def.Type = "Custom"; return this; } // Path stored?
        public IAxisVisualBuilder WithPivot(double x, double y) { _def.PivotX = (float)x; _def.PivotY = (float)y; return this; }
        public IAxisVisualBuilder WithSize(double w, double h) { _def.Width = (float)w; _def.Height = (float)h; return this; }
        public IAxisVisualBuilder Horizontal() { _def.IsVertical = false; return this; }
        public IAxisVisualBuilder Vertical() { _def.IsVertical = true; return this; }
        public IAxisVisualBuilder Reversed() { _def.IsReversed = true; return this; }
        public IAxisVisualBuilder Forward() { _def.IsReversed = false; return this; }
    }

    internal class CaptureCylinderBuilder : ICylinderVisualBuilder
    {
        private readonly VisualStyleDef _def;
        public CaptureCylinderBuilder(VisualStyleDef def) => _def = def;

        public ICylinderVisualBuilder AsSlideBlock(double? s) { _def.Type = "SlideBlock"; if(s.HasValue) _def.Param1 = s.Value; return this; }
        public ICylinderVisualBuilder AsGripper(double o, double c) { _def.Type = "Gripper"; _def.Param1 = o; _def.Param2 = c; return this; }
        public ICylinderVisualBuilder AsSuctionPen(double d) { _def.Type = "SuctionPen"; _def.Param1 = d; return this; }
        public ICylinderVisualBuilder AsCustom(string p) { _def.Type = "Custom"; return this; }
        public ICylinderVisualBuilder WithPivot(double x, double y) { _def.PivotX = (float)x; _def.PivotY = (float)y; return this; }
        public ICylinderVisualBuilder WithSize(double w, double h) { _def.Width = (float)w; _def.Height = (float)h; return this; }
        public ICylinderVisualBuilder Horizontal() { _def.IsVertical = false; return this; }
        public ICylinderVisualBuilder Vertical() { _def.IsVertical = true; return this; }
        public ICylinderVisualBuilder Reversed() { _def.IsReversed = true; return this; }
        public ICylinderVisualBuilder Forward() { _def.IsReversed = false; return this; }
    }

    internal class CaptureBindingBuilder : IBindingBuilder
    {
        private readonly VisualBindingDef _def;
        public CaptureBindingBuilder(VisualBindingDef def) => _def = def;
        public IBindingBuilder ToAxis(AxisID a) { _def.TargetDeviceNames.Add(a.Name); return this; }
        public IBindingBuilder ToCylinder(CylinderID c) { _def.TargetDeviceNames.Add(c.Name); return this; }
        public IBindingBuilder TargetRoot(string r) { _def.TargetRootName = r; return this; }
        public IBindingBuilder Vertical() => this;
        public IBindingBuilder Horizontal() => this;
        public IBindingBuilder Map(Func<double, object> m) => this;
    }

    // --- Static Helpers ---
    public static class UI
    {
        private static Func<object, IUIVisualizer> _factory = _ => new StubUIVisualizer();
        public static void UseFactory(Func<object, IUIVisualizer> factory) => _factory = factory;
        public static IUIVisualizer Link(object form) => _factory(form);
        public static IUIVisualizer CreateStub() => new StubUIVisualizer();
    }

    public static class Visuals
    {
        public static VisualLayout Start() => new VisualLayout();
        public static VisualLayout Define(Action<IDeviceVisualRegistry> config) { var l = new VisualLayout(); l.AddAction(config); return l; }
    }

    public sealed class VisualLayout
    {
        private readonly System.Collections.Generic.List<Action<IDeviceVisualRegistry>> _actions = new();
        internal void AddAction(Action<IDeviceVisualRegistry> action) => _actions.Add(action);
        public Action<IDeviceVisualRegistry> Build() => r => { foreach (var a in _actions) a(r); };

        public VisualAxisStyleBuilder For(AxisID axis) => new VisualAxisStyleBuilder(this, axis);
        public VisualCylinderStyleBuilder For(CylinderID cylinder) => new VisualCylinderStyleBuilder(this, cylinder);
        public VisualLayout AutoHighlight(object panel, DeviceID id) { AddAction(r => r.AutoHighlight(panel, id)); return this; }
        public VisualBindingBuilder Bind(object panel) => new VisualBindingBuilder(this, panel);
        public VisualLayout Select(Func<VisualLayout, VisualLayout> s) => s(this);
    }
    
    // ... [VisualAxisStyleBuilder etc. remain same as previous commit, no need to duplicate if they just call the interface] ...
    // Wait, the Fluent Builders (VisualAxisStyleBuilder) simply collect Actions and apply them to the Interface.
    // So they are compatible with ANY implementation of the Interface (including our new Capture one).
    // I will keep them but for brevity I assume they exist.
    
    public sealed class VisualAxisStyleBuilder
    {
        private readonly VisualLayout _layout;
        private readonly AxisID _axis;
        private readonly System.Collections.Generic.List<Action<IAxisVisualBuilder>> _steps = new();
        public VisualAxisStyleBuilder(VisualLayout l, AxisID a) { _layout = l; _axis = a; }
        public VisualAxisStyleBuilder AsLinearGuide(double l, double s) { _steps.Add(b => b.AsLinearGuide(l, s)); return this; }
        public VisualAxisStyleBuilder AsRotaryTable(double r) { _steps.Add(b => b.AsRotaryTable(r)); return this; }
        public VisualAxisStyleBuilder AsCustom(string p) { _steps.Add(b => b.AsCustom(p)); return this; }
        public VisualAxisStyleBuilder WithPivot(double x, double y) { _steps.Add(b => b.WithPivot(x, y)); return this; }
        public VisualAxisStyleBuilder WithSize(double w, double h) { _steps.Add(b => b.WithSize(w, h)); return this; }
        public VisualAxisStyleBuilder Horizontal() { _steps.Add(b => b.Horizontal()); return this; }
        public VisualAxisStyleBuilder Vertical() { _steps.Add(b => b.Vertical()); return this; }
        public VisualAxisStyleBuilder Reversed() { _steps.Add(b => b.Reversed()); return this; }
        public VisualLayout Done() { _layout.AddAction(r => { var b = r.For(_axis); foreach(var s in _steps) s(b); }); return _layout; }
    }
    // (Similar for VisualCylinderStyleBuilder and VisualBindingBuilder - omitted for brevity but strictly required for compilation)
    
     public sealed class VisualCylinderStyleBuilder
    {
        private readonly VisualLayout _layout;
        private readonly CylinderID _cyl;
        private readonly System.Collections.Generic.List<Action<ICylinderVisualBuilder>> _steps = new();
        public VisualCylinderStyleBuilder(VisualLayout l, CylinderID c) { _layout = l; _cyl = c; }
        public VisualCylinderStyleBuilder AsSlideBlock(double? s = null) { _steps.Add(b => b.AsSlideBlock(s)); return this; }
        public VisualCylinderStyleBuilder AsGripper(double o, double c) { _steps.Add(b => b.AsGripper(o, c)); return this; }
        public VisualCylinderStyleBuilder AsSuctionPen(double d) { _steps.Add(b => b.AsSuctionPen(d)); return this; }
        public VisualCylinderStyleBuilder AsCustom(string p) { _steps.Add(b => b.AsCustom(p)); return this; }
        public VisualCylinderStyleBuilder WithPivot(double x, double y) { _steps.Add(b => b.WithPivot(x, y)); return this; }
        public VisualCylinderStyleBuilder WithSize(double w, double h) { _steps.Add(b => b.WithSize(w, h)); return this; }
        public VisualCylinderStyleBuilder Horizontal() { _steps.Add(b => b.Horizontal()); return this; }
        public VisualCylinderStyleBuilder Vertical() { _steps.Add(b => b.Vertical()); return this; }
        public VisualCylinderStyleBuilder Reversed() { _steps.Add(b => b.Reversed()); return this; }
        public VisualLayout Done() { _layout.AddAction(r => { var b = r.For(_cyl); foreach(var s in _steps) s(b); }); return _layout; }
    }

    public sealed class VisualBindingBuilder
    {
        private readonly VisualLayout _layout;
        private readonly object _panel;
        private readonly System.Collections.Generic.List<Action<IBindingBuilder>> _steps = new();
        public VisualBindingBuilder(VisualLayout l, object p) { _layout = l; _panel = p; }
        public VisualBindingBuilder ToAxis(AxisID axis) { _steps.Add(b => b.ToAxis(axis)); return this; }
        public VisualBindingBuilder ToCylinder(CylinderID cylinder) { _steps.Add(b => b.ToCylinder(cylinder)); return this; }
        public VisualBindingBuilder TargetRoot(string r) { _steps.Add(b => b.TargetRoot(r)); return this; }
        public VisualBindingBuilder Vertical() { _steps.Add(b => b.Vertical()); return this; }
        public VisualBindingBuilder Horizontal() { _steps.Add(b => b.Horizontal()); return this; }
        public VisualLayout Done() { _layout.AddAction(r => { var b = r.Bind(_panel); foreach(var s in _steps) s(b); }); return _layout; }
        
        // LINQ Support
        public VisualLayout Select(Func<VisualBindingBuilder, VisualLayout> s) => s(this);
        public TResult SelectMany<TIntermediate, TResult>(
            Func<VisualBindingBuilder, TIntermediate> intermediateSelector,
            Func<VisualBindingBuilder, TIntermediate, TResult> resultSelector)
            => resultSelector(this, intermediateSelector(this));
    }


    internal class StubUIVisualizer : IUIVisualizer
    {
        public IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter i) => this;
        public IUIVisualizer ObserveContext(FlowContext c) => this;
        public IUIVisualizer Visuals(Action<IDeviceVisualRegistry> r) => this;
        public IBindingBuilder Bind(object p) => new StubBindingBuilder();
        public IUIVisualizer AutoHighlight(object p, DeviceID id) => this;
    }
    
    internal class StubBindingBuilder : IBindingBuilder
    {
        public IBindingBuilder ToAxis(AxisID a) => this;
        public IBindingBuilder ToCylinder(CylinderID c) => this;
        public IBindingBuilder TargetRoot(string r) => this;
        public IBindingBuilder Vertical() => this;
        public IBindingBuilder Horizontal() => this;
        public IBindingBuilder Map(Func<double, object> m) => this;
    }
}
