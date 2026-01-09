using System;
using System.Collections.Generic;
using cszmcaux; // 引用正运动官方提供的C#封装类
using Devices.Motion.Abstractions;
using Common.Core;

namespace Devices.Motion.Implementations.Zaux
{
    /// <summary>
    /// 正运动控制器实现类
    /// </summary>
    /// <typeparam name="TAxis">轴枚举类型</typeparam>
    /// <typeparam name="TIn">输入枚举类型</typeparam>
    /// <typeparam name="TOut">输出枚举类型</typeparam>
    public class ZauxMotionController<TAxis, TIn, TOut> : IMotionController<TAxis, TIn, TOut>
    {
        public string IP { get; set; } = "192.168.0.11";

        public ushort CardNo { get; set; }

        public Action<ushort> InitDelegate { get; set; }

        private IntPtr _handle = IntPtr.Zero;

        private static void CheckResult(int result)
        {
            if (result != 0)
            {
                throw new ZauxException(result);
            }
        }

        private static int ToInt32(object value, string paramName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(paramName);
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"{paramName} 无法转换为 int: {value}", paramName, ex);
            }
        }

        public void AxisEnable(TAxis axis, Level enable)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            CheckResult(zmcaux.ZAux_Direct_SetAxisEnable(_handle, axisNo, (int)enable));
        }

        public bool CheckDone(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            int idle = 0;
            // 参见 PDF 指令39: ZAux_Direct_GetIfIdle
            // 0: 运动中, -1: 停止
            int ret = zmcaux.ZAux_Direct_GetIfIdle(_handle, axisNo, ref idle);
            if (ret != 0) return true;
            return idle == -1;
        }

        public bool CheckHomeDone(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            uint status = 0;
            // 参见 PDF 指令111: ZAux_Direct_GetHomeStatus
            zmcaux.ZAux_Direct_GetHomeStatus(_handle, axisNo, ref status);
            return status == 1;
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                // 参见 PDF 指令5: ZAux_Close
                zmcaux.ZAux_Close(_handle);
                _handle = IntPtr.Zero;
            }
        }

        public Level GetAxisAlarm(TAxis axis)
        {
            var status = GetAxisStatus(axis);
            return status.ServoAlarm;
        }

        public AxisStatus GetAxisStatus(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            int status = 0;
            // 参见 PDF 指令40: ZAux_Direct_GetAxisStatus
            // 返回值详细说明参见 PDF 5.1.2-1 和 9.1.3-1
            CheckResult(zmcaux.ZAux_Direct_GetAxisStatus(_handle, axisNo, ref status));

            AxisStatus s = new AxisStatus();

            // 默认置 Off
            s.ServoAlarm = Level.Off;
            s.PositiveHardLimit = Level.Off;
            s.NegativeHardLimit = Level.Off;
            s.EmergencyStop = Level.Off;
            s.Origin = Level.Off;

            // Bit 1 (2): 随动误差超限 (FE)
            // Bit 3 (8): 远程驱动器报错
            // Bit 22 (4194304): 告警信号输入 (ALM)
            s.ServoAlarm = ((status & 2) != 0 || (status & 8) != 0 || (status & 4194304) != 0) ? Level.On : Level.Off;

            // Bit 4 (16): 正向硬限位
            s.PositiveHardLimit = (status & 16) != 0 ? Level.On : Level.Off;

            // Bit 5 (32): 负向硬限位
            s.NegativeHardLimit = (status & 32) != 0 ? Level.On : Level.Off;

            // Origin: 检查轴的原点输入
            int orgIn = -1;
            zmcaux.ZAux_Direct_GetDatumIn(_handle, axisNo, ref orgIn);
            if (orgIn >= 0)
            {
                uint val = 0;
                zmcaux.ZAux_Direct_GetIn(_handle, orgIn, ref val);
                s.Origin = (val == 1) ? Level.On : Level.Off;
            }

            // EmergencyStop: 状态字无专用急停位，通常与ALM并列
            s.EmergencyStop = s.ServoAlarm;

            return s;
        }

        public double GetCommandPos(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            float pos = 0;
            // 参见 PDF 指令27: ZAux_Direct_GetDpos
            CheckResult(zmcaux.ZAux_Direct_GetDpos(_handle, axisNo, ref pos));
            return pos;
        }

        public double GetEncoderPos(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            float pos = 0;
            // 参见 PDF 指令29: ZAux_Direct_GetMpos
            CheckResult(zmcaux.ZAux_Direct_GetMpos(_handle, axisNo, ref pos));
            return pos;
        }

        public Level GetInput(TIn bitNo)
        {
            int bit = ToInt32(bitNo, nameof(bitNo));
            uint val = 0;
            // 参见 PDF 指令197: ZAux_Direct_GetIn
            CheckResult(zmcaux.ZAux_Direct_GetIn(_handle, bit, ref val));
            return val == 0 ? Level.Off : Level.On;
        }

        public int GetOutput(TOut bitNo)
        {
            int bit = ToInt32(bitNo, nameof(bitNo));
            uint val = 0;
            // 参见 PDF 指令199: ZAux_Direct_GetOp
            CheckResult(zmcaux.ZAux_Direct_GetOp(_handle, bit, ref val));
            return (int)val;
        }

        public double GetSpeed(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            float speed = 0;
            // 参见 PDF 指令25: ZAux_Direct_GetSpeed
            CheckResult(zmcaux.ZAux_Direct_GetSpeed(_handle, axisNo, ref speed));
            return speed;
        }

        public void GoBackHome(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令103: ZAux_Direct_Single_Datum
            // 模式3: 双向找原点+找Z
            CheckResult(zmcaux.ZAux_Direct_Single_Datum(_handle, axisNo, 3));
        }

        public void Initialization()
        {
            if (_handle != IntPtr.Zero)
            {
                zmcaux.ZAux_Close(_handle);
                _handle = IntPtr.Zero;
            }

            int ret = zmcaux.ZAux_OpenEth(IP, out _handle);
            if (ret != 0 || _handle == IntPtr.Zero)
            {
                CheckResult(ret);
                throw new InvalidOperationException("ZAux_OpenEth 失败，句柄为空");
            }

            InitDelegate?.Invoke(CardNo);
        }

        public void Move_Absolute(TAxis axis, double pos)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令93: ZAux_Direct_Single_MoveAbs
            CheckResult(zmcaux.ZAux_Direct_Single_MoveAbs(_handle, axisNo, (float)pos));
        }

        public void Move_JOG(TAxis axis, MotionDirection dir)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令94: ZAux_Direct_Single_Vmove
            // dir: 1 正向, -1 负向
            int direction = dir == MotionDirection.Negative ? -1 : 1;
            CheckResult(zmcaux.ZAux_Direct_Single_Vmove(_handle, axisNo, direction));
        }

        public void Move_Relative(TAxis axis, double pos)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令92: ZAux_Direct_Single_Move
            CheckResult(zmcaux.ZAux_Direct_Single_Move(_handle, axisNo, (float)pos));
        }

        public void SetCommandPos(TAxis axis, double pos)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令26: ZAux_Direct_SetDpos
            CheckResult(zmcaux.ZAux_Direct_SetDpos(_handle, axisNo, (float)pos));
        }

        public void SetEncoderPos(TAxis axis, double pos)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令28: ZAux_Direct_SetMpos
            CheckResult(zmcaux.ZAux_Direct_SetMpos(_handle, axisNo, (float)pos));
        }

        public void SetOutput(TOut bitNo, Level level)
        {
            int bit = ToInt32(bitNo, nameof(bitNo));
            // 参见 PDF 指令198: ZAux_Direct_SetOp
            CheckResult(zmcaux.ZAux_Direct_SetOp(_handle, bit, (uint)level));
        }

        public void SetSpeed(TAxis axis, AxisSpeed speed)
        {
            int axisNo = ToInt32(axis, nameof(axis));

            // 1. 起始速度 (PDF 指令33: ZAux_Direct_SetLspeed)
            CheckResult(zmcaux.ZAux_Direct_SetLspeed(_handle, axisNo, (float)speed.Min));

            // 2. 运行速度 (PDF 指令24: ZAux_Direct_SetSpeed)
            CheckResult(zmcaux.ZAux_Direct_SetSpeed(_handle, axisNo, (float)speed.Max));

            // 3. 加速度 (PDF 指令20: ZAux_Direct_SetAccel)
            float accel = speed.Tacc > 0 ? (float)(speed.Max / speed.Tacc) : (float)speed.Max * 1000;
            CheckResult(zmcaux.ZAux_Direct_SetAccel(_handle, axisNo, accel));

            // 4. 减速度 (PDF 指令22: ZAux_Direct_SetDecel)
            float decel = speed.Tdec > 0 ? (float)(speed.Max / speed.Tdec) : (float)speed.Max * 1000;
            CheckResult(zmcaux.ZAux_Direct_SetDecel(_handle, axisNo, decel));

            // 5. S曲线平滑时间 (PDF 指令35: ZAux_Direct_SetSramp) - 文档单位 ms
            CheckResult(zmcaux.ZAux_Direct_SetSramp(_handle, axisNo, (float)(speed.S_Para * 1000)));

            // 6. 急停减速度 (PDF 指令32: ZAux_Direct_SetFastDec)
            CheckResult(zmcaux.ZAux_Direct_SetFastDec(_handle, axisNo, decel * 2));
        }

        public void Stop(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令189: ZAux_Direct_Single_Cancel
            // Mode 2: 取消当前运动 + 缓冲运动
            CheckResult(zmcaux.ZAux_Direct_Single_Cancel(_handle, axisNo, 2));
        }

        public void EStop(TAxis axis)
        {
            int axisNo = ToInt32(axis, nameof(axis));
            // 参见 PDF 指令189: ZAux_Direct_Single_Cancel
            // Mode 3: 立即停止
            CheckResult(zmcaux.ZAux_Direct_Single_Cancel(_handle, axisNo, 3));
        }
    }
}
