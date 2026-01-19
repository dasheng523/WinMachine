using System;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Flow;

namespace Machine.Framework.Core.Simulation
{
    public interface IVisualFlowInterpreter : Machine.Framework.Core.Flow.IFlowInterpreter
    {
        IObservable<ActiveStepUpdate> TraceStream { get; }
    }

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
        IBindingBuilder Vertical();
        IBindingBuilder Horizontal();
        IBindingBuilder Map(Func<double, object> mapper);
    }

    public interface IAxisVisualBuilder
    {
        IAxisVisualBuilder AsLinearGuide(double length, double sliderWidth);
        IAxisVisualBuilder AsRotaryTable(double radius);
        IAxisVisualBuilder AsCustom(string modelPath);
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
        ICylinderVisualBuilder Vertical();
        ICylinderVisualBuilder Horizontal();
        ICylinderVisualBuilder Reversed();
        ICylinderVisualBuilder Forward();
    }

    public enum StepStatus { Ready, Running, Completed, Error }
    public record ActiveStepUpdate(string TargetDevice, string Name, StepStatus Status);

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

        public static VisualLayout Define(Action<IDeviceVisualRegistry> config)
        {
            var layout = new VisualLayout();
            layout.AddAction(config);
            return layout;
        }
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

    public sealed class VisualBindingBuilder
    {
        private readonly VisualLayout _layout;
        private readonly object _panel;
        private readonly System.Collections.Generic.List<Action<IBindingBuilder>> _steps = new();
        public VisualBindingBuilder(VisualLayout l, object p) { _layout = l; _panel = p; }
        public VisualBindingBuilder ToAxis(AxisID axis) { _steps.Add(b => b.ToAxis(axis)); return this; }
        public VisualBindingBuilder ToCylinder(CylinderID cylinder) { _steps.Add(b => b.ToCylinder(cylinder)); return this; }
        public VisualBindingBuilder Vertical() { _steps.Add(b => b.Vertical()); return this; }
        public VisualBindingBuilder Horizontal() { _steps.Add(b => b.Horizontal()); return this; }
        public VisualLayout Done() { _layout.AddAction(r => { var b = r.Bind(_panel); foreach(var s in _steps) s(b); }); return _layout; }
    }

    public sealed class VisualAxisStyleBuilder
    {
        private readonly VisualLayout _layout;
        private readonly AxisID _axis;
        private readonly System.Collections.Generic.List<Action<IAxisVisualBuilder>> _steps = new();
        public VisualAxisStyleBuilder(VisualLayout l, AxisID a) { _layout = l; _axis = a; }
        public VisualAxisStyleBuilder AsLinearGuide(double l, double s) { _steps.Add(b => b.AsLinearGuide(l, s)); return this; }
        public VisualAxisStyleBuilder AsRotaryTable(double r) { _steps.Add(b => b.AsRotaryTable(r)); return this; }
        public VisualAxisStyleBuilder Horizontal() { _steps.Add(b => b.Horizontal()); return this; }
        public VisualAxisStyleBuilder Vertical() { _steps.Add(b => b.Vertical()); return this; }
        public VisualAxisStyleBuilder Reversed() { _steps.Add(b => b.Reversed()); return this; }
        public VisualLayout Done() { _layout.AddAction(r => { var b = r.For(_axis); foreach(var s in _steps) s(b); }); return _layout; }
    }

    public sealed class VisualCylinderStyleBuilder
    {
        private readonly VisualLayout _layout;
        private readonly CylinderID _cyl;
        private readonly System.Collections.Generic.List<Action<ICylinderVisualBuilder>> _steps = new();
        public VisualCylinderStyleBuilder(VisualLayout l, CylinderID c) { _layout = l; _cyl = c; }
        public VisualCylinderStyleBuilder AsSlideBlock(double? size = null) { _steps.Add(b => b.AsSlideBlock(size)); return this; }
        public VisualCylinderStyleBuilder AsGripper(double o, double c) { _steps.Add(b => b.AsGripper(o, c)); return this; }
        public VisualCylinderStyleBuilder AsSuctionPen(double d) { _steps.Add(b => b.AsSuctionPen(d)); return this; }
        public VisualCylinderStyleBuilder Horizontal() { _steps.Add(b => b.Horizontal()); return this; }
        public VisualCylinderStyleBuilder Vertical() { _steps.Add(b => b.Vertical()); return this; }
        public VisualCylinderStyleBuilder Reversed() { _steps.Add(b => b.Reversed()); return this; }
        public VisualLayout Done() { _layout.AddAction(r => { var b = r.For(_cyl); foreach(var s in _steps) s(b); }); return _layout; }
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
        public IBindingBuilder Vertical() => this;
        public IBindingBuilder Horizontal() => this;
        public IBindingBuilder Map(Func<double, object> m) => this;
    }
}
