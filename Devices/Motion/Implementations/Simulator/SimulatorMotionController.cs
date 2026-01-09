using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Devices.Motion.Abstractions;
using Common.Core;

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

        private int ToInt(object value) => Convert.ToInt32(value);

        private MockAxisData GetAxisData(TAxis axis)
        {
            int index = ToInt(axis);
            if (!_axes.ContainsKey(index))
                _axes[index] = new MockAxisData();
            return _axes[index];
        }

        public void AxisEnable(TAxis axis, Level enable)
        {
            GetAxisData(axis).Enabled = enable;
        }

        public bool CheckDone(TAxis axis)
        {
            return !GetAxisData(axis).IsMoving;
        }

        public bool CheckHomeDone(TAxis axis)
        {
            return true;
        }

        public void Dispose()
        {
        }

        public Level GetAxisAlarm(TAxis axis)
        {
            return Level.Off;
        }

        public AxisStatus GetAxisStatus(TAxis axis)
        {
            return new AxisStatus
            {
                ServoAlarm = Level.Off,
                PositiveHardLimit = Level.Off,
                NegativeHardLimit = Level.Off,
                EmergencyStop = Level.Off,
                Origin = Level.Off
            };
        }

        public double GetCommandPos(TAxis axis)
        {
            return GetAxisData(axis).Position;
        }

        public double GetEncoderPos(TAxis axis)
        {
            return GetAxisData(axis).Position;
        }

        public Level GetInput(TIn bitNo)
        {
            int bit = ToInt(bitNo);
            return _inputs.ContainsKey(bit) ? _inputs[bit] : Level.Off;
        }

        public int GetOutput(TOut bitNo)
        {
            int bit = ToInt(bitNo);
            return _outputs.ContainsKey(bit) ? (int)_outputs[bit] : 0;
        }

        public double GetSpeed(TAxis axis)
        {
            return GetAxisData(axis).Speed.Max;
        }

        public void GoBackHome(TAxis axis)
        {
            var data = GetAxisData(axis);
            data.Position = 0;
        }

        public void Initialization()
        {
        }

        public void Move_Absolute(TAxis axis, double pos)
        {
            var data = GetAxisData(axis);
            data.IsMoving = true;
            Task.Run(async () =>
            {
                await Task.Delay(500);
                data.Position = pos;
                data.IsMoving = false;
            });
        }

        public void Move_JOG(TAxis axis, MotionDirection dir)
        {
            GetAxisData(axis).IsMoving = true;
        }

        public void Move_Relative(TAxis axis, double pos)
        {
            var data = GetAxisData(axis);
            data.IsMoving = true;
            Task.Run(async () =>
            {
                await Task.Delay(500);
                data.Position += pos;
                data.IsMoving = false;
            });
        }

        public void SetCommandPos(TAxis axis, double pos)
        {
            GetAxisData(axis).Position = pos;
        }

        public void SetEncoderPos(TAxis axis, double pos)
        {
            GetAxisData(axis).Position = pos;
        }

        public void SetOutput(TOut bitNo, Level level)
        {
            _outputs[ToInt(bitNo)] = level;
        }

        public void SetSpeed(TAxis axis, AxisSpeed speed)
        {
            GetAxisData(axis).Speed = speed;
        }

        public void Stop(TAxis axis)
        {
            GetAxisData(axis).IsMoving = false;
        }

        public void EStop(TAxis axis)
        {
            GetAxisData(axis).IsMoving = false;
        }
    }
}
