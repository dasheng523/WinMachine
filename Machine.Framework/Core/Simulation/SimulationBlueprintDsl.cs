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
            return new StubAssemblyBuilder(name);
        }

        // --- 内部 Stub 实现，仅用于原型编译与演示 ---

        private class StubAssemblyBuilder : ISimulatorAssemblyBuilder
        {
            public string Name { get; }
            public StubAssemblyBuilder(string name) => Name = name;

            public IBoardBuilder AddBoard(string name, int cardId) => new StubBoardBuilder();

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
                var board = new StubBoardBuilder();
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

        private class StubBoardBuilder : IBoardBuilder
        {
            public IAxisBuilder AddAxis(int id, string name) => new StubAxisBuilder();
            public ICylinderBuilder AddCylinder(string name, int doOut, int doIn) => new StubCylinderBuilder();

            public IBoardBuilder AddAxis(int id, string name, Action<IAxisBuilder> configure)
            {
                var axis = new StubAxisBuilder();
                configure(axis);
                return this;
            }

            public IBoardBuilder AddCylinder(string name, int doOut, int doIn, Action<ICylinderBuilder> configure)
            {
                var cyl = new StubCylinderBuilder();
                configure(cyl);
                return this;
            }
        }

        private class StubAxisBuilder : IAxisBuilder
        {
            public IAxisBuilder WithKinematics(double maxVel, double maxAcc) => this;
            public IAxisBuilder WithRange(double min, double max) => this;
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

        private class StubCylinderBuilder : ICylinderBuilder
        {
            public ICylinderBuilder WithFeedback(int diOut, int diIn) => this;
            public ICylinderBuilder WithDynamics(int actionTimeMs) => this;
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
            return Machine.Framework.Core.Configuration.Models.MachineConfig.Create();
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
