using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Primitives;
using LanguageExt;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Devices.Implementations.Simulator
{
    public class SimulatorMotionController<TAxis, TIn, TOut> : IMotionController<TAxis, TIn, TOut>
        where TAxis : notnull
        where TIn : notnull
        where TOut : notnull
    {
        // 管理所有仿真轴实例
        private readonly ConcurrentDictionary<TAxis, SimulatorAxis> _axes = new();
        private readonly ConcurrentDictionary<TOut, Level> _outputs = new();
        private readonly ConcurrentDictionary<TIn, Level> _inputs = new();

        /// <summary>
        /// 获取仿真轴对象 (用于绑定 UI)
        /// </summary>
        public SimulatorAxis GetAxis(TAxis axisId)
        {
            return _axes.GetOrAdd(axisId, id => 
                new SimulatorAxis(id.ToString()!, 0, 1000, 200)); // 默认行程0-1000, 速度200
        }
        
        // 暴露给DSL使用的非泛型API (如果有需要)
        public ISimulatorAxis GetSimulatorAxis(TAxis axis) => GetAxis(axis);

        public Fin<LUnit> Initialization()
        {
            return FinSucc(unit);
        }

        public Fin<LUnit> AxisEnable(TAxis axis, Level enable)
        {
            // 仿真仅记录状态
            return FinSucc(unit);
        }

        public Fin<bool> CheckDone(TAxis axis)
        {
            var ax = GetAxis(axis);
            // 如果不在运动中，即为完成
            bool done = !ax.CurrentState.IsMoving;
            return FinSucc(done);
        }

        public Fin<bool> CheckHomeDone(TAxis axis)
        {
            return FinSucc(GetAxis(axis).CurrentState.IsHomed);
        }

        public Fin<Level> GetAxisAlarm(TAxis axis)
        {
            return FinSucc(Level.Off);
        }

        public Fin<AxisStatus> GetAxisStatus(TAxis axis)
        {
            var ax = GetAxis(axis);
            var state = ax.CurrentState;
            return FinSucc(new AxisStatus
            {
                Moving = state.IsMoving,
                Error = false,
                Origin = state.IsHomed ? Level.On : Level.Off,
                PositiveHardLimit = Level.Off,
                NegativeHardLimit = Level.Off
            });
        }

        public Fin<double> GetCommandPos(TAxis axis)
        {
            return FinSucc(GetAxis(axis).CurrentState.CommandPos);
        }

        public Fin<double> GetEncoderPos(TAxis axis)
        {
            return FinSucc(GetAxis(axis).CurrentState.Position);
        }

        public Fin<Level> GetInput(TIn bitNo)
        {
            return FinSucc(_inputs.TryGetValue(bitNo, out var lvl) ? lvl : Level.Off);
        }

        public Fin<Level> GetOutput(TOut bitNo)
        {
            return FinSucc(_outputs.TryGetValue(bitNo, out var lvl) ? lvl : Level.Off);
        }

        public Fin<LUnit> GoBackHome(TAxis axis)
        {
            var ax = GetAxis(axis);
            // 简单仿真回原点：直接运行到0
            ax.StartMove(0, 50); 
            return FinSucc(unit);
        }

        public Fin<LUnit> Move_Absolute(TAxis axis, double pos)
        {
            var ax = GetAxis(axis);
            ax.StartMove(pos, ax.CurrentState.Speed.Max);
            return FinSucc(unit);
        }

        public Fin<LUnit> Move_JOG(TAxis axis, MotionDirection dir)
        {
            // JOG 仿真：向极限位置运动
            var ax = GetAxis(axis);
            double target = dir == MotionDirection.Positive ? ax.TravelMax : ax.TravelMin;
            ax.StartMove(target, ax.CurrentState.Speed.Max);
            return FinSucc(unit);
        }

        public Fin<LUnit> Move_Relative(TAxis axis, double delta)
        {
            var ax = GetAxis(axis);
            double target = ax.CurrentState.Position + delta;
            ax.StartMove(target, ax.CurrentState.Speed.Max);
            return FinSucc(unit);
        }

        public Fin<LUnit> SetCommandPos(TAxis axis, double pos)
        {
            GetAxis(axis).SetLogicalPosition(pos);
            return FinSucc(unit);
        }

        public Fin<LUnit> SetEncoderPos(TAxis axis, double pos)
        {
            GetAxis(axis).SetLogicalPosition(pos);
            return FinSucc(unit);
        }

        public Fin<LUnit> SetOutput(TOut bitNo, Level level)
        {
            _outputs[bitNo] = level;
            return FinSucc(unit);
        }

        public Fin<AxisSpeed> GetSpeed(TAxis axis)
        {
            return FinSucc(GetAxis(axis).CurrentState.Speed);
        }

        public Fin<LUnit> SetSpeed(TAxis axis, AxisSpeed speed)
        {
            // 更新 Axis 的速度参数 (需要扩展 SimulatorAxis 支持设置速度，目前简化)
            // GetAxis(axis).UpdateSpeedConfig(speed); 
            return FinSucc(unit);
        }

        public Fin<LUnit> Stop(TAxis axis)
        {
            GetAxis(axis).Stop();
            return FinSucc(unit);
        }

        public Fin<LUnit> EStop(TAxis axis)
        {
            GetAxis(axis).Stop();
            return FinSucc(unit);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
