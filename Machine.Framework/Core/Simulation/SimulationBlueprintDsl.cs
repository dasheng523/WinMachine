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
            public IMountPointBuilder Mount(string name) => new StubMountPointBuilder();

            // --- 实例方法优先于扩展方法，解决 LINQ 歧义 ---
            public ISimulatorAssemblyBuilder Select(Func<ISimulatorAssemblyBuilder, ISimulatorAssemblyBuilder> selector) => selector(this);
            
            public TResult SelectMany<TIntermediate, TResult>(
                Func<ISimulatorAssemblyBuilder, TIntermediate> intermediateSelector,
                Func<ISimulatorAssemblyBuilder, TIntermediate, TResult> resultSelector)
            {
                var intermediate = intermediateSelector(this);
                return resultSelector(this, intermediate);
            }

            // 支持 let (匿名类型处理)
            public TResult SelectMany<TIntermediate, TResult>(
                Func<ISimulatorAssemblyBuilder, TIntermediate> intermediateSelector,
                Func<object, TIntermediate, TResult> resultSelector)
            {
                // 注意：在 Stub 实现中，我们目前不真正处理匿名对象的解构，仅为原型编译成功
                var intermediate = intermediateSelector(this);
                return resultSelector(this, intermediate);
            }
        }

        private class StubBoardBuilder : IBoardBuilder
        {
            public IAxisBuilder AddAxis(int id, string name) => new StubAxisBuilder();
            public ICylinderBuilder AddCylinder(string name, int doOut, int doIn) => new StubCylinderBuilder();
        }

        private class StubAxisBuilder : IAxisBuilder
        {
            public IAxisBuilder WithKinematics(double maxVel, double maxAcc) => this;
            public IAxisBuilder WithRange(double min, double max) => this;
        }

        private class StubMountPointBuilder : IMountPointBuilder
        {
            public IMountPointBuilder AttachedTo(object parent) => this;
            public IMountPointBuilder LinkTo(object axis) => this;
            public IMountPointBuilder WithTransform(Func<double, double> transform) => this;
            public IMountPointBuilder WithOffset(double x = 0, double y = 0, double z = 0) => this;
        }

        private class StubCylinderBuilder : ICylinderBuilder
        {
            public ICylinderBuilder WithFeedback(int diOut, int diIn) => this;
            public ICylinderBuilder WithDynamics(int actionTimeMs) => this;
        }
    }

    // --- DSL 契约接口定义 ---

    public interface ISimulatorAssemblyBuilder
    {
        string Name { get; }
        IBoardBuilder AddBoard(string name, int cardId);
        IMountPointBuilder Mount(string name);

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
    }

    public interface IAxisBuilder
    {
        IAxisBuilder WithKinematics(double maxVel, double maxAcc);
        IAxisBuilder WithRange(double min, double max);
    }

    public interface IMountPointBuilder
    {
        /// <summary>
        /// 定义挂载父级（实现层级拓扑）
        /// </summary>
        IMountPointBuilder AttachedTo(object parent);

        /// <summary>
        /// 定义联动轴
        /// </summary>
        IMountPointBuilder LinkTo(object axis);

        /// <summary>
        /// 定义轴位置到挂载点位移的变换函数
        /// </summary>
        IMountPointBuilder WithTransform(Func<double, double> transform);

        /// <summary>
        /// 物理位移偏置
        /// </summary>
        IMountPointBuilder WithOffset(double x = 0, double y = 0, double z = 0);
    }

    public interface ICylinderBuilder
    {
        ICylinderBuilder WithFeedback(int diOut, int diIn);
        ICylinderBuilder WithDynamics(int actionTimeMs);
    }

    /// <summary>
    /// 专门的解释逻辑工具，负责将蓝图描述转换为可执行的对象。
    /// 保持 DSL 纯净，转换逻辑外部化。
    /// </summary>
    public static class BlueprintInterpreter
    {
        public static Machine.Framework.Core.Configuration.Models.MachineConfig ToConfig(ISimulatorAssemblyBuilder blueprint)
        {
            // 实际上会遍历 blueprint 内部的配置树并生成 MachineConfig
            // 目前返回一个 Stub 对象
            return Machine.Framework.Core.Configuration.Models.MachineConfig.Create();
        }

        public static object ToRuntime(ISimulatorAssemblyBuilder blueprint)
        {
            // 创建真正的物理模拟引擎实例
            return new object(); // SimulationRuntime
        }
    }

    // --- 视觉绑定与踪迹流相关的契约 ---

    public enum StepStatus { Ready, Running, Completed, Error }

    public record ActiveStepUpdate(string TargetDevice, string Name, StepStatus Status);

    /// <summary>
    /// 可视化流程解释器，支持 Rx.NET 踪迹流
    /// </summary>
    public interface IVisualFlowInterpreter : Machine.Framework.Core.Flow.IFlowInterpreter
    {
        IObservable<ActiveStepUpdate> TraceStream { get; }
    }

    /// <summary>
    /// UI 绑定器 DSL 接口
    /// </summary>
    public interface IUIVisualizer
    {
        IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter);
        IUIVisualizer AutoHighlight(object panel, string deviceId);
        IBindingBuilder Bind(object panel);
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

    // --- Stub 实现 ---
    internal class StubUIVisualizer : IUIVisualizer
    {
        public IUIVisualizer ObserveInterpreter(IVisualFlowInterpreter interpreter) => this;
        public IUIVisualizer AutoHighlight(object panel, string deviceId) => this;
        public IBindingBuilder Bind(object panel) => new StubBindingBuilder();
    }

    internal class StubBindingBuilder : IBindingBuilder
    {
        public IBindingBuilder ToAxis(string axisId) => this;
        public IBindingBuilder Vertical() => this;
        public IBindingBuilder Horizontal() => this;
        public IBindingBuilder Map(Func<double, object> mapper) => this;
    }
}
