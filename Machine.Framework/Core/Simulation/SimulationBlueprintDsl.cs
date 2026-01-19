using System;
using System.Collections.Generic;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Core.Simulation
{
    /// <summary>
    /// 模拟器蓝图 DSL 入口点
    /// </summary>
    public static class MachineSimulator
    {
        /// <summary>
        /// 开始组装一台模拟机器
        /// </summary>
        public static ISimulatorAssemblyBuilder Assemble(string name)
        {
            return new BlueprintAssemblyBuilder(name);
        }

        internal sealed record BlueprintAxisDefinition(
            int Id,
            string Name,
            double Min,
            double Max,
            double MaxVel,
            double MaxAcc);

        internal sealed record BlueprintCylinderDefinition(
            string Name,
            int DoOut,
            int DoIn,
            int? FeedbackDiOut,
            int? FeedbackDiIn,
            int ActionTimeMs);

        internal sealed class BlueprintAssembly
        {
            public string Name { get; }
            public List<BlueprintAxisDefinition> Axes { get; } = new();
            public List<BlueprintCylinderDefinition> Cylinders { get; } = new();

            public BlueprintAssembly(string name) => Name = name;
        }

        // --- 内部实现：保留 LINQ 外观，但把信息落到 BlueprintAssembly ---

        internal sealed class BlueprintAssemblyBuilder : ISimulatorAssemblyBuilder
        {
            public string Name { get; }
            internal BlueprintAssembly Assembly { get; }

            public BlueprintAssemblyBuilder(string name)
            {
                Name = name;
                Assembly = new BlueprintAssembly(name);
            }

            public IBoardBuilder AddBoard(string name, int cardId) => new BlueprintBoardBuilder(Assembly, name, cardId);

            public ISimulatorAssemblyBuilder Select(Func<ISimulatorAssemblyBuilder, ISimulatorAssemblyBuilder> selector) => selector(this);
            
            public TResult SelectMany<TIntermediate, TResult>(
                Func<ISimulatorAssemblyBuilder, TIntermediate> intermediateSelector,
                Func<ISimulatorAssemblyBuilder, TIntermediate, TResult> resultSelector)
            {
                var intermediate = intermediateSelector(this);
                return resultSelector(this, intermediate);
            }

            public TResult SelectMany<TIntermediate, TResult>(
                Func<ISimulatorAssemblyBuilder, TIntermediate> intermediateSelector,
                Func<object, TIntermediate, TResult> resultSelector)
            {
                var intermediate = intermediateSelector(this);
                return resultSelector(this, intermediate);
            }

            public ISimulatorAssemblyBuilder AddBoard(string name, int cardId, Action<IBoardBuilder> configure)
            {
                var board = new BlueprintBoardBuilder(Assembly, name, cardId);
                configure(board);
                return this;
            }

            public IMountPointBuilder Mount(string name) => new StubMountPointBuilder();
            public ISimulatorAssemblyBuilder Mount(string name, Action<IMountPointBuilder> configure)
            {
                var mount = new StubMountPointBuilder();
                configure(mount);
                return this;
            }
        }

        private sealed class BlueprintBoardBuilder : IBoardBuilder
        {
            private readonly BlueprintAssembly _assembly;
            private readonly string _boardName;
            private readonly int _cardId;

            public BlueprintBoardBuilder(BlueprintAssembly assembly, string boardName, int cardId)
            {
                _assembly = assembly;
                _boardName = boardName;
                _cardId = cardId;
            }

            public IAxisBuilder AddAxis(int id, AxisID axis)
            {
                var builder = new BlueprintAxisBuilder(id, axis.Name, _assembly);
                builder.Commit();
                return builder;
            }

            public ICylinderBuilder AddCylinder(CylinderID cylinder, int doOut, int doIn)
            {
                var cyl = new BlueprintCylinderBuilder(cylinder.Name, doOut, doIn, _assembly);
                cyl.Commit();
                return cyl;
            }

            public IBoardBuilder AddAxis(int id, AxisID axis, Action<IAxisBuilder> configure)
            {
                var builder = new BlueprintAxisBuilder(id, axis.Name, _assembly);
                configure(builder);
                builder.Commit();
                return this;
            }

            public IBoardBuilder AddCylinder(CylinderID cylinder, int doOut, int doIn, Action<ICylinderBuilder> configure)
            {
                var cyl = new BlueprintCylinderBuilder(cylinder.Name, doOut, doIn, _assembly);
                configure(cyl);
                cyl.Commit();
                return this;
            }
        }

        private sealed class BlueprintAxisBuilder : IAxisBuilder
        {
            private readonly int _id;
            private readonly string _name;
            private readonly BlueprintAssembly _assembly;

            private double _min = 0;
            private double _max = 1000;
            private double _maxVel = 200;
            private double _maxAcc = 200;

            public BlueprintAxisBuilder(int id, string name, BlueprintAssembly assembly)
            {
                _id = id;
                _name = name;
                _assembly = assembly;
            }

            public IAxisBuilder WithKinematics(double maxVel, double maxAcc)
            {
                _maxVel = maxVel;
                _maxAcc = maxAcc;
                return this;
            }

            public IAxisBuilder WithRange(double min, double max)
            {
                _min = min;
                _max = max;
                return this;
            }

            public void Commit()
            {
                if (_assembly.Axes.Exists(a => a.Name == _name)) return;
                _assembly.Axes.Add(new BlueprintAxisDefinition(_id, _name, _min, _max, _maxVel, _maxAcc));
            }
        }

        private class StubMountPointBuilder : IMountPointBuilder
        {
            public IMountPointBuilder AttachedTo(object parent) => this;
            public IMountPointBuilder AttachedTo(string parentName) => this;
            public IMountPointBuilder LinkTo(object axis) => this;
            public IMountPointBuilder LinkTo(DeviceID id) => this;
            public IMountPointBuilder WithTransform(Func<double, double> transform) => this;
            public IMountPointBuilder WithOffset(double x = 0, double y = 0, double z = 0) => this;
            public IMountPointBuilder Mount(string name, Action<IMountPointBuilder> configure)
            {
                var child = new StubMountPointBuilder();
                configure(child);
                return this;
            }
            public IMountPointBuilder Mount(string name) => new StubMountPointBuilder();
        }

        private sealed class BlueprintCylinderBuilder : ICylinderBuilder
        {
            private readonly string _name;
            private readonly int _doOut;
            private readonly int _doIn;
            private readonly BlueprintAssembly _assembly;

            private int? _fbDiOut;
            private int? _fbDiIn;
            private int _actionTimeMs = 200;

            public BlueprintCylinderBuilder(string name, int doOut, int doIn, BlueprintAssembly assembly)
            {
                _name = name;
                _doOut = doOut;
                _doIn = doIn;
                _assembly = assembly;
            }

            public ICylinderBuilder WithFeedback(int diOut, int diIn)
            {
                _fbDiOut = diOut;
                _fbDiIn = diIn;
                return this;
            }

            public ICylinderBuilder WithDynamics(int actionTimeMs)
            {
                _actionTimeMs = actionTimeMs;
                return this;
            }

            public void Commit()
            {
                if (_assembly.Cylinders.Exists(c => c.Name == _name)) return;
                _assembly.Cylinders.Add(new BlueprintCylinderDefinition(_name, _doOut, _doIn, _fbDiOut, _fbDiIn, _actionTimeMs));
            }
        }
    }

    // --- 视觉 Builder Stub ---

    internal class StubAxisVisualBuilder : IAxisVisualBuilder
    {
        public IAxisVisualBuilder AsLinearGuide(double length, double sliderWidth) => this;
        public IAxisVisualBuilder AsRotaryTable(double radius) => this;
        public IAxisVisualBuilder AsCustom(string modelPath) => this;

        public IAxisVisualBuilder Horizontal() => this;
        public IAxisVisualBuilder Vertical() => this;
        public IAxisVisualBuilder Forward() => this;
        public IAxisVisualBuilder Reversed() => this;
    }

    internal class StubCylinderVisualBuilder : ICylinderVisualBuilder
    {
        public ICylinderVisualBuilder AsSlideBlock(double? blockSize = null) => this;
        public ICylinderVisualBuilder AsGripper(double openWidth, double closeWidth) => this;
        public ICylinderVisualBuilder AsSuctionPen(double diameter) => this;
        public ICylinderVisualBuilder AsCustom(string modelPath) => this;

        public ICylinderVisualBuilder Horizontal() => this;
        public ICylinderVisualBuilder Vertical() => this;
        public ICylinderVisualBuilder Forward() => this;
        public ICylinderVisualBuilder Reversed() => this;
    }

    // --- DSL 契约接口定义 ---

    public interface ISimulatorAssemblyBuilder
    {
        string Name { get; }
        IBoardBuilder AddBoard(string name, int cardId);
        ISimulatorAssemblyBuilder AddBoard(string name, int cardId, Action<IBoardBuilder> configure);
        
        IMountPointBuilder Mount(string name);
        ISimulatorAssemblyBuilder Mount(string name, Action<IMountPointBuilder> configure);

        // LINQ Support
        ISimulatorAssemblyBuilder Select(Func<ISimulatorAssemblyBuilder, ISimulatorAssemblyBuilder> selector);
        
        TResult SelectMany<TIntermediate, TResult>(
            Func<ISimulatorAssemblyBuilder, TIntermediate> intermediateSelector,
            Func<ISimulatorAssemblyBuilder, TIntermediate, TResult> resultSelector);

        TResult SelectMany<TIntermediate, TResult>(
            Func<ISimulatorAssemblyBuilder, TIntermediate> intermediateSelector,
            Func<object, TIntermediate, TResult> resultSelector);
    }

    public interface IBoardBuilder
    {
        IAxisBuilder AddAxis(int id, AxisID axis);
        ICylinderBuilder AddCylinder(CylinderID cylinder, int doOut, int doIn);

        IBoardBuilder AddAxis(int id, AxisID axis, Action<IAxisBuilder> configure);
        IBoardBuilder AddCylinder(CylinderID cylinder, int doOut, int doIn, Action<ICylinderBuilder> configure);
    }

    public interface IAxisBuilder
    {
        IAxisBuilder WithKinematics(double maxVel, double maxAcc);
        IAxisBuilder WithRange(double min, double max);
    }

    public interface IAxisVisualBuilder
    {
        IAxisVisualBuilder AsLinearGuide(double length, double sliderWidth);
        IAxisVisualBuilder AsRotaryTable(double radius);
        IAxisVisualBuilder AsCustom(string modelPath);

        IAxisVisualBuilder Horizontal();
        IAxisVisualBuilder Vertical();
        IAxisVisualBuilder Forward();
        IAxisVisualBuilder Reversed();
    }

    public interface IMountPointBuilder
    {
        IMountPointBuilder AttachedTo(object parent);
        IMountPointBuilder AttachedTo(string parentName);

        IMountPointBuilder LinkTo(object axis);
        IMountPointBuilder LinkTo(DeviceID id);

        IMountPointBuilder Mount(string name, Action<IMountPointBuilder> configure);
        IMountPointBuilder Mount(string name);

        IMountPointBuilder WithTransform(Func<double, double> transform);
        IMountPointBuilder WithOffset(double x = 0, double y = 0, double z = 0);
    }

    public interface ICylinderBuilder
    {
        ICylinderBuilder WithFeedback(int diOut, int diIn);
        ICylinderBuilder WithDynamics(int actionTimeMs);
    }

    public interface ICylinderVisualBuilder
    {
        ICylinderVisualBuilder AsSlideBlock(double? blockSize = null);
        ICylinderVisualBuilder AsGripper(double openWidth, double closeWidth);
        ICylinderVisualBuilder AsSuctionPen(double diameter);
        ICylinderVisualBuilder AsCustom(string modelPath);

        ICylinderVisualBuilder Horizontal();
        ICylinderVisualBuilder Vertical();
        ICylinderVisualBuilder Forward();
        ICylinderVisualBuilder Reversed();
    }

    public static class BlueprintInterpreter
    {
        public static Machine.Framework.Core.Configuration.Models.MachineConfig ToConfig(ISimulatorAssemblyBuilder blueprint)
        {
            var config = Machine.Framework.Core.Configuration.Models.MachineConfig.Create();

            if (blueprint is not MachineSimulator.BlueprintAssemblyBuilder b)
                return config;

            var asm = b.Assembly;

            if (asm.Axes.Count > 0 || asm.Cylinders.Count > 0)
            {
                config.AddControlBoard("SimBoard", board =>
                {
                    foreach (var axis in asm.Axes)
                    {
                        board.MapAxis(axis.Name, axis.Id);
                    }

                    foreach (var cyl in asm.Cylinders)
                    {
                        if (cyl.FeedbackDiOut.HasValue && cyl.FeedbackDiIn.HasValue)
                        {
                            board.MapCylinder(cyl.Name, cyl.DoOut, cyl.FeedbackDiOut.Value, cyl.FeedbackDiIn.Value);
                        }
                        else
                        {
                            board.MapCylinder(cyl.Name, cyl.DoOut);
                        }
                    }

                    board.UseSimulator();
                });

                foreach (var axis in asm.Axes)
                {
                    config.ConfigureAxis(axis.Name, a => a.SetSoftLimits(sl => sl.Range(axis.Min, axis.Max)));
                }

                foreach (var cyl in asm.Cylinders)
                {
                    config.ConfigureCylinder(cyl.Name, c => c.MoveTime = cyl.ActionTimeMs);
                }

                config.UseSimulator("SimBoard", sim =>
                {
                    foreach (var axis in asm.Axes)
                    {
                        sim.Axis(axis.Name, a => a.Travel(axis.Min, axis.Max));
                    }
                });
            }

            return config;
        }

        public static object ToRuntime(ISimulatorAssemblyBuilder blueprint)
        {
            return new object();
        }
    }

    public enum StepStatus { Ready, Running, Completed, Error }

    public record ActiveStepUpdate(string TargetDevice, string Name, StepStatus Status);

    public interface IVisualFlowInterpreter : Machine.Framework.Core.Flow.IFlowInterpreter
    {
        IObservable<ActiveStepUpdate> TraceStream { get; }
    }

    public interface IUIVisualizer
    {
        IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter);
        IUIVisualizer ObserveContext(Machine.Framework.Core.Flow.FlowContext context);
        IUIVisualizer AutoHighlight(object panel, DeviceID id);
        IUIVisualizer Visuals(Action<IDeviceVisualRegistry> registryConfig);
        IBindingBuilder Bind(object panel);
    }

    public interface IDeviceVisualRegistry
    {
        IDeviceVisualRegistry AutoHighlight(object panel, DeviceID id);
        IBindingBuilder Bind(object panel);
        IAxisVisualBuilder ForAxis(AxisID axis);
        ICylinderVisualBuilder ForCylinder(CylinderID cylinder);
    }

    public interface IBindingBuilder
    {
        IBindingBuilder ToAxis(AxisID axis);
        IBindingBuilder ToCylinder(CylinderID cylinder);
        IBindingBuilder Vertical();
        IBindingBuilder Horizontal();
        IBindingBuilder Map(Func<double, object> mapper);
    }

    public static class UI
    {
        private static Func<object, IUIVisualizer> _factory = _ => new StubUIVisualizer();

        public static void UseFactory(Func<object, IUIVisualizer> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public static IUIVisualizer Link(object form) => _factory(form);

        public static IUIVisualizer CreateStub() => new StubUIVisualizer();
    }

    public static class Visuals
    {
        public static VisualLayout Start() => new VisualLayout();

        public static VisualLayout Define(Action<IDeviceVisualRegistry> registryConfig)
        {
            if (registryConfig == null) throw new ArgumentNullException(nameof(registryConfig));
            var layout = new VisualLayout();
            layout.AddAction(registryConfig);
            return layout;
        }
    }

    public sealed class VisualLayout
    {
        private readonly System.Collections.Generic.List<Action<IDeviceVisualRegistry>> _actions = new();

        internal void AddAction(Action<IDeviceVisualRegistry> action)
        {
            if (action == null) return;
            _actions.Add(action);
        }

        internal Action<IDeviceVisualRegistry> Build()
        {
            return registry =>
            {
                foreach (var action in _actions)
                {
                    action(registry);
                }
            };
        }

        public VisualLayout AutoHighlight(object panel, DeviceID id)
        {
            AddAction(v => v.AutoHighlight(panel, id));
            return this;
        }

        public VisualBindingBuilder Bind(object panel) => new VisualBindingBuilder(this, panel);

        public VisualAxisStyleBuilder ForAxis(AxisID axis) => new VisualAxisStyleBuilder(this, axis);

        public VisualCylinderStyleBuilder ForCylinder(CylinderID cylinder) => new VisualCylinderStyleBuilder(this, cylinder);

        public VisualLayout SelectMany(Func<VisualLayout, VisualLayout> selector, Func<VisualLayout, VisualLayout, VisualLayout> resultSelector)
        {
            var next = selector(this);
            return resultSelector(this, next);
        }

        public VisualLayout Select(Func<VisualLayout, VisualLayout> selector) => selector(this);
    }

    public sealed class VisualAxisStyleBuilder
    {
        private readonly VisualLayout _layout;
        private readonly AxisID _axis;
        private readonly System.Collections.Generic.List<Action<IAxisVisualBuilder>> _steps = new();

        internal VisualAxisStyleBuilder(VisualLayout layout, AxisID axis)
        {
            _layout = layout;
            _axis = axis;
        }

        public VisualAxisStyleBuilder AsLinearGuide(double length, double sliderWidth)
        {
            _steps.Add(b => b.AsLinearGuide(length, sliderWidth));
            return this;
        }

        public VisualAxisStyleBuilder AsRotaryTable(double radius)
        {
            _steps.Add(b => b.AsRotaryTable(radius));
            return this;
        }

        public VisualAxisStyleBuilder AsCustom(string modelPath)
        {
            _steps.Add(b => b.AsCustom(modelPath));
            return this;
        }

        public VisualAxisStyleBuilder Horizontal()
        {
            _steps.Add(b => b.Horizontal());
            return this;
        }

        public VisualAxisStyleBuilder Vertical()
        {
            _steps.Add(b => b.Vertical());
            return this;
        }

        public VisualAxisStyleBuilder Forward()
        {
            _steps.Add(b => b.Forward());
            return this;
        }

        public VisualAxisStyleBuilder Reversed()
        {
            _steps.Add(b => b.Reversed());
            return this;
        }

        public VisualLayout Done()
        {
            _layout.AddAction(v =>
            {
                var builder = v.ForAxis(_axis);
                foreach (var step in _steps)
                {
                    step(builder);
                }
            });

            return _layout;
        }
    }

    public sealed class VisualCylinderStyleBuilder
    {
        private readonly VisualLayout _layout;
        private readonly CylinderID _cylinder;
        private readonly System.Collections.Generic.List<Action<ICylinderVisualBuilder>> _steps = new();

        internal VisualCylinderStyleBuilder(VisualLayout layout, CylinderID cylinder)
        {
            _layout = layout;
            _cylinder = cylinder;
        }

        public VisualCylinderStyleBuilder AsSlideBlock(double? blockSize = null)
        {
            _steps.Add(b => b.AsSlideBlock(blockSize));
            return this;
        }

        public VisualCylinderStyleBuilder AsGripper(double openWidth, double closeWidth)
        {
            _steps.Add(b => b.AsGripper(openWidth, closeWidth));
            return this;
        }

        public VisualCylinderStyleBuilder AsSuctionPen(double diameter)
        {
            _steps.Add(b => b.AsSuctionPen(diameter));
            return this;
        }

        public VisualCylinderStyleBuilder AsCustom(string modelPath)
        {
            _steps.Add(b => b.AsCustom(modelPath));
            return this;
        }

        public VisualCylinderStyleBuilder Horizontal()
        {
            _steps.Add(b => b.Horizontal());
            return this;
        }

        public VisualCylinderStyleBuilder Vertical()
        {
            _steps.Add(b => b.Vertical());
            return this;
        }

        public VisualCylinderStyleBuilder Forward()
        {
            _steps.Add(b => b.Forward());
            return this;
        }

        public VisualCylinderStyleBuilder Reversed()
        {
            _steps.Add(b => b.Reversed());
            return this;
        }

        public VisualLayout Done()
        {
            _layout.AddAction(v =>
            {
                var builder = v.ForCylinder(_cylinder);
                foreach (var step in _steps)
                {
                    step(builder);
                }
            });

            return _layout;
        }
    }

    public sealed class VisualBindingBuilder
    {
        private readonly VisualLayout _layout;
        private readonly object _panel;
        private readonly System.Collections.Generic.List<Action<IBindingBuilder>> _steps = new();

        internal VisualBindingBuilder(VisualLayout layout, object panel)
        {
            _layout = layout;
            _panel = panel;
        }

        public VisualBindingBuilder ToAxis(AxisID axis)
        {
            _steps.Add(b => b.ToAxis(axis));
            return this;
        }

        public VisualBindingBuilder ToCylinder(CylinderID cylinder)
        {
            _steps.Add(b => b.ToCylinder(cylinder));
            return this;
        }

        public VisualBindingBuilder Vertical()
        {
            _steps.Add(b => b.Vertical());
            return this;
        }

        public VisualBindingBuilder Horizontal()
        {
            _steps.Add(b => b.Horizontal());
            return this;
        }

        public VisualBindingBuilder Map(Func<double, object> mapper)
        {
            _steps.Add(b => b.Map(mapper));
            return this;
        }

        public VisualLayout Done()
        {
            _layout.AddAction(v =>
            {
                var builder = v.Bind(_panel);
                foreach (var step in _steps)
                {
                    step(builder);
                }
            });
            return _layout;
        }
    }

    public static class VisualInterpreterExtensions
    {
        public static IUIVisualizer AttachVisuals(this IVisualFlowInterpreter interpreter, object form, Machine.Framework.Core.Flow.FlowContext context, VisualLayout layout)
        {
            if (interpreter == null) throw new ArgumentNullException(nameof(interpreter));
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            return UI.Link(form)
                .ObserveInterpreter(interpreter)
                .ObserveContext(context)
                .Visuals(layout.Build());
        }
    }

    internal class StubUIVisualizer : IUIVisualizer
    {
        public IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter) => this;
        public IUIVisualizer ObserveContext(Machine.Framework.Core.Flow.FlowContext context) => this;
        public IUIVisualizer AutoHighlight(object panel, DeviceID id) => this;
        public IUIVisualizer Visuals(Action<IDeviceVisualRegistry> registryConfig)
        {
            var registry = new StubDeviceVisualRegistry();
            registryConfig(registry);
            return this;
        }
        public IBindingBuilder Bind(object panel) => new StubBindingBuilder();
    }

    internal class StubDeviceVisualRegistry : IDeviceVisualRegistry
    {
        public IDeviceVisualRegistry AutoHighlight(object panel, DeviceID id) => this;
        public IBindingBuilder Bind(object panel) => new StubBindingBuilder();
        public IAxisVisualBuilder ForAxis(AxisID axis) => new StubAxisVisualBuilder();
        public ICylinderVisualBuilder ForCylinder(CylinderID cylinder) => new StubCylinderVisualBuilder();
    }

    internal class StubBindingBuilder : IBindingBuilder
    {
        public IBindingBuilder ToAxis(AxisID axis) => this;
        public IBindingBuilder ToCylinder(CylinderID cylinder) => this;
        public IBindingBuilder Vertical() => this;
        public IBindingBuilder Horizontal() => this;
        public IBindingBuilder Map(Func<double, object> mapper) => this;
    }
}
