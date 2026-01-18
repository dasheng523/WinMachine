using System;
using System.Collections.Generic;

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

            public IAxisBuilder AddAxis(int id, string name)
            {
                var axis = new BlueprintAxisBuilder(id, name, _assembly);
                axis.Commit();
                return axis;
            }

            public ICylinderBuilder AddCylinder(string name, int doOut, int doIn)
            {
                var cyl = new BlueprintCylinderBuilder(name, doOut, doIn, _assembly);
                cyl.Commit();
                return cyl;
            }

            public IBoardBuilder AddAxis(int id, string name, Action<IAxisBuilder> configure)
            {
                var axis = new BlueprintAxisBuilder(id, name, _assembly);
                configure(axis);
                axis.Commit();
                return this;
            }

            public IBoardBuilder AddCylinder(string name, int doOut, int doIn, Action<ICylinderBuilder> configure)
            {
                var cyl = new BlueprintCylinderBuilder(name, doOut, doIn, _assembly);
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
            public IMountPointBuilder LinkTo(string deviceId) => this;
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
        public ICylinderVisualBuilder AsSlider(double width, double height) => this;
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
        IAxisBuilder AddAxis(int id, string name);
        ICylinderBuilder AddCylinder(string name, int doOut, int doIn);

        IBoardBuilder AddAxis(int id, string name, Action<IAxisBuilder> configure);
        IBoardBuilder AddCylinder(string name, int doOut, int doIn, Action<ICylinderBuilder> configure);
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
        IMountPointBuilder LinkTo(string deviceId);

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
        ICylinderVisualBuilder AsSlider(double width, double height);
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
        IUIVisualizer AutoHighlight(object panel, string deviceId);
        IUIVisualizer Visuals(Action<IDeviceVisualRegistry> registryConfig);
        IBindingBuilder Bind(object panel);
    }

    public interface IDeviceVisualRegistry
    {
        IAxisVisualBuilder ForAxis(string axisId);
        ICylinderVisualBuilder ForCylinder(string cylinderId);
    }

    public interface IBindingBuilder
    {
        IBindingBuilder ToAxis(string axisId);
        IBindingBuilder Vertical();
        IBindingBuilder Horizontal();
        IBindingBuilder Map(Func<double, object> mapper);
    }

    public static class UI
    {
        public static IUIVisualizer Link(object form) => new StubUIVisualizer();
    }

    internal class StubUIVisualizer : IUIVisualizer
    {
        public IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter) => this;
        public IUIVisualizer AutoHighlight(object panel, string deviceId) => this;
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
        public IAxisVisualBuilder ForAxis(string axisId) => new StubAxisVisualBuilder();
        public ICylinderVisualBuilder ForCylinder(string cylinderId) => new StubCylinderVisualBuilder();
    }

    internal class StubBindingBuilder : IBindingBuilder
    {
        public IBindingBuilder ToAxis(string axisId) => this;
        public IBindingBuilder Vertical() => this;
        public IBindingBuilder Horizontal() => this;
        public IBindingBuilder Map(Func<double, object> mapper) => this;
    }
}
