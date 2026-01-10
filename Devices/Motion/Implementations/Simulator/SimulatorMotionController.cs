using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Devices.Motion.Abstractions;
using Common.Core;
using LanguageExt;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Devices.Motion.Implementations.Simulator
{
    /// <summary>
    /// 模拟器控制器实现类
    /// </summary>
    /// <typeparam name="TAxis">轴枚举类型</typeparam>
    /// <typeparam name="TIn">输入枚举类型</typeparam>
    /// <typeparam name="TOut">输出枚举类型</typeparam>
    public class SimulatorMotionController<TAxis, TIn, TOut> : IMotionController<TAxis, TIn, TOut>
    {
        private class MockAxisData
        {
            public double Position;
            public AxisSpeed Speed;
            public bool IsMoving;
            public Level Enabled;
        }

        private readonly Dictionary<int, MockAxisData> _axes = new Dictionary<int, MockAxisData>();
        private readonly Dictionary<int, Level> _inputs = new Dictionary<int, Level>();
        private readonly Dictionary<int, Level> _outputs = new Dictionary<int, Level>();

        private int ToInt(object? value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return Convert.ToInt32(value);
        }

        private MockAxisData GetAxisData(TAxis axis)
        {
            int index = ToInt(axis);
            if (!_axes.ContainsKey(index))
                _axes[index] = new MockAxisData();
            return _axes[index];
        }

        public Fin<LUnit> AxisEnable(TAxis axis, Level enable)
        {
            GetAxisData(axis).Enabled = enable;
            return FinSucc(unit);
        }

        public Fin<bool> CheckDone(TAxis axis)
        {
            return FinSucc(!GetAxisData(axis).IsMoving);
        }

        public Fin<bool> CheckHomeDone(TAxis axis)
        {
            return FinSucc(true);
        }

        public void Dispose()
        {
        }

        public Fin<Level> GetAxisAlarm(TAxis axis)
        {
            return FinSucc(Level.Off);
        }

        public Fin<AxisStatus> GetAxisStatus(TAxis axis)
        {
            return FinSucc(new AxisStatus
            {
                ServoAlarm = Level.Off,
                PositiveHardLimit = Level.Off,
                NegativeHardLimit = Level.Off,
                EmergencyStop = Level.Off,
                Origin = Level.Off
            });
        }

        public Fin<double> GetCommandPos(TAxis axis)
        {
            return FinSucc(GetAxisData(axis).Position);
        }

        public Fin<double> GetEncoderPos(TAxis axis)
        {
            return FinSucc(GetAxisData(axis).Position);
        }

        public Fin<Level> GetInput(TIn bitNo)
        {
            int bit = ToInt(bitNo);
            return FinSucc(_inputs.ContainsKey(bit) ? _inputs[bit] : Level.Off);
        }

        public Fin<int> GetOutput(TOut bitNo)
        {
            int bit = ToInt(bitNo);
            return FinSucc(_outputs.ContainsKey(bit) ? (int)_outputs[bit] : 0);
        }

        public Fin<double> GetSpeed(TAxis axis)
        {
            return FinSucc(GetAxisData(axis).Speed.Max);
        }

        public Fin<LUnit> GoBackHome(TAxis axis)
        {
            var data = GetAxisData(axis);
            data.Position = 0;
            return FinSucc(unit);
        }

        public Fin<LUnit> Initialization() => FinSucc(unit);

        public Fin<LUnit> Move_Absolute(TAxis axis, double pos)
        {
            var data = GetAxisData(axis);
            data.IsMoving = true;
            Task.Run(async () =>
            {
                await Task.Delay(500);
                data.Position = pos;
                data.IsMoving = false;
            });
            return FinSucc(unit);
        }

        public Fin<LUnit> Move_JOG(TAxis axis, MotionDirection dir)
        {
            GetAxisData(axis).IsMoving = true;
            return FinSucc(unit);
        }

        public Fin<LUnit> Move_Relative(TAxis axis, double pos)
        {
            var data = GetAxisData(axis);
            data.IsMoving = true;
            Task.Run(async () =>
            {
                await Task.Delay(500);
                data.Position += pos;
                data.IsMoving = false;
            });
            return FinSucc(unit);
        }

        public Fin<LUnit> SetCommandPos(TAxis axis, double pos)
        {
            GetAxisData(axis).Position = pos;
            return FinSucc(unit);
        }

        public Fin<LUnit> SetEncoderPos(TAxis axis, double pos)
        {
            GetAxisData(axis).Position = pos;
            return FinSucc(unit);
        }

        public Fin<LUnit> SetOutput(TOut bitNo, Level level)
        {
            _outputs[ToInt(bitNo)] = level;
            return FinSucc(unit);
        }

        public Fin<LUnit> SetSpeed(TAxis axis, AxisSpeed speed)
        {
            GetAxisData(axis).Speed = speed;
            return FinSucc(unit);
        }

        public Fin<LUnit> Stop(TAxis axis)
        {
            GetAxisData(axis).IsMoving = false;
            return FinSucc(unit);
        }

        public Fin<LUnit> EStop(TAxis axis)
        {
            GetAxisData(axis).IsMoving = false;
            return FinSucc(unit);
        }
    }
}
