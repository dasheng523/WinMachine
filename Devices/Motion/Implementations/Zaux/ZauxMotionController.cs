using System;
using System.Collections.Generic;
using cszmcaux; // 引用正运动官方提供的C#封装类
using Devices.Motion.Abstractions;
using Common.Core;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

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

        public Action<ushort>? InitDelegate { get; set; }

        private IntPtr _handle = IntPtr.Zero;

        private static void CheckResult(int result)
        {
            if (result != 0)
            {
                throw new ZauxException(result);
            }
        }

        private static int ToInt32(object? value, string paramName)
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

        private static Error MotionError(string op, Exception ex) =>
            Error.New(new Exception($"Zaux.{op} 失败: {ex.Message}", ex));

        public Fin<LUnit> AxisEnable(TAxis axis, Level enable)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                CheckResult(zmcaux.ZAux_Direct_SetAxisEnable(_handle, axisNo, (int)enable));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"AxisEnable({axis})", ex));
            }
        }

        public Fin<bool> CheckDone(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                int idle = 0;
                // 参见 PDF 指令39: ZAux_Direct_GetIfIdle
                // 0: 运动中, -1: 停止
                int ret = zmcaux.ZAux_Direct_GetIfIdle(_handle, axisNo, ref idle);
                if (ret != 0) return FinSucc(true);
                return FinSucc(idle == -1);
            }
            catch (Exception ex)
            {
                return FinFail<bool>(MotionError($"CheckDone({axis})", ex));
            }
        }

        public Fin<bool> CheckHomeDone(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                uint status = 0;
                // 参见 PDF 指令111: ZAux_Direct_GetHomeStatus
                zmcaux.ZAux_Direct_GetHomeStatus(_handle, axisNo, ref status);
                return FinSucc(status == 1);
            }
            catch (Exception ex)
            {
                return FinFail<bool>(MotionError($"CheckHomeDone({axis})", ex));
            }
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

        public Fin<Level> GetAxisAlarm(TAxis axis)
        {
            return
                from status in GetAxisStatus(axis)
                select status.ServoAlarm;
        }

        public Fin<AxisStatus> GetAxisStatus(TAxis axis)
        {
            try
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

                return FinSucc(s);
            }
            catch (Exception ex)
            {
                return FinFail<AxisStatus>(MotionError($"GetAxisStatus({axis})", ex));
            }
        }

        public Fin<double> GetCommandPos(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                float pos = 0;
                // 参见 PDF 指令27: ZAux_Direct_GetDpos
                CheckResult(zmcaux.ZAux_Direct_GetDpos(_handle, axisNo, ref pos));
                return FinSucc((double)pos);
            }
            catch (Exception ex)
            {
                return FinFail<double>(MotionError($"GetCommandPos({axis})", ex));
            }
        }

        public Fin<double> GetEncoderPos(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                float pos = 0;
                // 参见 PDF 指令29: ZAux_Direct_GetMpos
                CheckResult(zmcaux.ZAux_Direct_GetMpos(_handle, axisNo, ref pos));
                return FinSucc((double)pos);
            }
            catch (Exception ex)
            {
                return FinFail<double>(MotionError($"GetEncoderPos({axis})", ex));
            }
        }

        public Fin<Level> GetInput(TIn bitNo)
        {
            try
            {
                int bit = ToInt32(bitNo, nameof(bitNo));
                uint val = 0;
                // 参见 PDF 指令197: ZAux_Direct_GetIn
                CheckResult(zmcaux.ZAux_Direct_GetIn(_handle, bit, ref val));
                return FinSucc(val == 0 ? Level.Off : Level.On);
            }
            catch (Exception ex)
            {
                return FinFail<Level>(MotionError($"GetInput({bitNo})", ex));
            }
        }

        public Fin<Level> GetOutput(TOut bitNo)
        {
            try
            {
                int bit = ToInt32(bitNo, nameof(bitNo));
                uint val = 0;
                // 参见 PDF 指令199: ZAux_Direct_GetOp
                CheckResult(zmcaux.ZAux_Direct_GetOp(_handle, bit, ref val));
                return FinSucc(val == 0 ? Level.Off : Level.On);
            }
            catch (Exception ex)
            {
                return FinFail<Level>(MotionError($"GetOutput({bitNo})", ex));
            }
        }

        public Fin<double> GetSpeed(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                float speed = 0;
                // 参见 PDF 指令25: ZAux_Direct_GetSpeed
                CheckResult(zmcaux.ZAux_Direct_GetSpeed(_handle, axisNo, ref speed));
                return FinSucc((double)speed);
            }
            catch (Exception ex)
            {
                return FinFail<double>(MotionError($"GetSpeed({axis})", ex));
            }
        }

        public Fin<LUnit> GoBackHome(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令103: ZAux_Direct_Single_Datum
                // 模式3: 双向找原点+找Z
                CheckResult(zmcaux.ZAux_Direct_Single_Datum(_handle, axisNo, 3));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"GoBackHome({axis})", ex));
            }
        }

        public Fin<LUnit> Initialization()
        {
            try
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
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError("Initialization", ex));
            }
        }

        public Fin<LUnit> Move_Absolute(TAxis axis, double pos)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令93: ZAux_Direct_Single_MoveAbs
                CheckResult(zmcaux.ZAux_Direct_Single_MoveAbs(_handle, axisNo, (float)pos));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"Move_Absolute({axis})", ex));
            }
        }

        public Fin<LUnit> Move_JOG(TAxis axis, MotionDirection dir)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令94: ZAux_Direct_Single_Vmove
                // dir: 1 正向, -1 负向
                int direction = dir == MotionDirection.Negative ? -1 : 1;
                CheckResult(zmcaux.ZAux_Direct_Single_Vmove(_handle, axisNo, direction));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"Move_JOG({axis})", ex));
            }
        }

        public Fin<LUnit> Move_Relative(TAxis axis, double pos)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令92: ZAux_Direct_Single_Move
                CheckResult(zmcaux.ZAux_Direct_Single_Move(_handle, axisNo, (float)pos));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"Move_Relative({axis})", ex));
            }
        }

        public Fin<LUnit> SetCommandPos(TAxis axis, double pos)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令26: ZAux_Direct_SetDpos
                CheckResult(zmcaux.ZAux_Direct_SetDpos(_handle, axisNo, (float)pos));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"SetCommandPos({axis})", ex));
            }
        }

        public Fin<LUnit> SetEncoderPos(TAxis axis, double pos)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令28: ZAux_Direct_SetMpos
                CheckResult(zmcaux.ZAux_Direct_SetMpos(_handle, axisNo, (float)pos));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"SetEncoderPos({axis})", ex));
            }
        }

        public Fin<LUnit> SetOutput(TOut bitNo, Level level)
        {
            try
            {
                int bit = ToInt32(bitNo, nameof(bitNo));
                // 参见 PDF 指令198: ZAux_Direct_SetOp
                CheckResult(zmcaux.ZAux_Direct_SetOp(_handle, bit, (uint)level));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"SetOutput({bitNo})", ex));
            }
        }

        public Fin<LUnit> SetSpeed(TAxis axis, AxisSpeed speed)
        {
            try
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
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"SetSpeed({axis})", ex));
            }
        }

        public Fin<LUnit> Stop(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令189: ZAux_Direct_Single_Cancel
                // Mode 2: 取消当前运动 + 缓冲运动
                CheckResult(zmcaux.ZAux_Direct_Single_Cancel(_handle, axisNo, 2));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"Stop({axis})", ex));
            }
        }

        public Fin<LUnit> EStop(TAxis axis)
        {
            try
            {
                int axisNo = ToInt32(axis, nameof(axis));
                // 参见 PDF 指令189: ZAux_Direct_Single_Cancel
                // Mode 3: 立即停止
                CheckResult(zmcaux.ZAux_Direct_Single_Cancel(_handle, axisNo, 3));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"EStop({axis})", ex));
            }
        }
    }
}
