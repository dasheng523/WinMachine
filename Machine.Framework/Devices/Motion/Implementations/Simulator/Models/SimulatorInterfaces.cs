using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Machine.Framework.Devices.Motion.Abstractions;

namespace Machine.Framework.Devices.Motion.Implementations.Simulator.Models
{
    /// <summary>
    /// 轴的瞬时状态快照
    /// </summary>
    public record AxisState
    {
        public double Position { get; init; }
        public double CommandPos { get; init; }
        public bool IsMoving { get; init; }
        public bool IsHomed { get; init; }
        public AxisSpeed Speed { get; init; }
    }

    /// <summary>
    /// 仿真设备基础接口
    /// </summary>
    public interface ISimulatorDevice<TState>
    {
        /// <summary>
        /// 状态变化流 (Hot Observable)
        /// </summary>
        IObservable<TState> StateStream { get; }

        /// <summary>
        /// 当前状态快照
        /// </summary>
        TState CurrentState { get; }
    }

    /// <summary>
    /// 仿真轴接口 (对应用户说的 MockAxis)
    /// </summary>
    public interface ISimulatorAxis : ISimulatorDevice<AxisState>
    {
        // --- 物理规格 ---
        string AxisId { get; }
        double TravelMin { get; }
        double TravelMax { get; }
        double MaxSpeed { get; }

        // --- 仿真行为 ---
        void SetLogicalPosition(double pos);
        void StartMove(double targetPos, double speed);
        void Stop();
    }
}
