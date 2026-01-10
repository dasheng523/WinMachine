using System;
using System.Collections.Generic;
using Devices.Motion.Abstractions;
using Common.Core;
using Leadshine;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Devices.Motion.Implementations.Leadshine
{
    /// <summary>
    /// 雷赛控制器实现类
    /// </summary>
    /// <typeparam name="TAxis">轴枚举类型</typeparam>
    /// <typeparam name="TIn">输入枚举类型</typeparam>
    /// <typeparam name="TOut">输出枚举类型</typeparam>
    public class LeadshineMotionController<TAxis, TIn, TOut> : IMotionController<TAxis, TIn, TOut>
    {
        public ushort CardNo { get; set; } = 0;

        public string IP { get; set; } = "192.168.5.11";

        public Action<ushort>? InitDelegate { get; set; }

        private bool _isConnected;

        public LeadshineMotionController(string ip, ushort cardNo, Action<ushort>? initDelegate = null)
        {
            IP = ip;
            CardNo = cardNo;
            InitDelegate = initDelegate;
        }

        private static void CheckResult(short result)
        {
            if (result != 0)
            {
                throw new LeadshineException(result);
            }
        }

        private static ushort ToUShort(object? value, string paramName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(paramName);
            }

            try
            {
                return Convert.ToUInt16(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"{paramName} 无法转换为 ushort: {value}", paramName, ex);
            }
        }

        private static Error MotionError(string op, Exception ex) =>
            Error.New(new Exception($"Leadshine.{op} 失败: {ex.Message}", ex));

        public Fin<LUnit> AxisEnable(TAxis axis, Level enable)
        {
            try
            {
                CheckResult(LTSMC.smc_write_sevon_pin(CardNo, ToUShort(axis, nameof(axis)), (ushort)enable));
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
                // smc_check_done：0-运行中，1-停止
                return FinSucc(LTSMC.smc_check_done(CardNo, ToUShort(axis, nameof(axis))) == 1);
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
                ushort state = 0;
                CheckResult(LTSMC.smc_get_home_result(CardNo, ToUShort(axis, nameof(axis)), ref state));
                return FinSucc(state == 1);
            }
            catch (Exception ex)
            {
                return FinFail<bool>(MotionError($"CheckHomeDone({axis})", ex));
            }
        }

        public void Dispose()
        {
            if (_isConnected)
            {
                LTSMC.smc_board_close(CardNo);
                _isConnected = false;
            }
        }

        public Fin<Level> GetAxisAlarm(TAxis axis)
        {
            try
            {
                // smc_read_alarm_pin：一般返回 0/1
                short state = LTSMC.smc_read_alarm_pin(CardNo, ToUShort(axis, nameof(axis)));
                return FinSucc(state == 0 ? Level.Off : Level.On);
            }
            catch (Exception ex)
            {
                return FinFail<Level>(MotionError($"GetAxisAlarm({axis})", ex));
            }
        }

        public Fin<AxisStatus> GetAxisStatus(TAxis axis)
        {
            try
            {
                uint status = LTSMC.smc_axis_io_status(CardNo, ToUShort(axis, nameof(axis)));
                AxisStatus iost = new AxisStatus();

                // 雷赛 IO 状态位映射
                iost.ServoAlarm = (status & (1u << 0)) == 0 ? Level.Off : Level.On;
                iost.PositiveHardLimit = (status & (1u << 1)) == 0 ? Level.Off : Level.On;
                iost.NegativeHardLimit = (status & (1u << 2)) == 0 ? Level.Off : Level.On;
                iost.EmergencyStop = (status & (1u << 3)) == 0 ? Level.Off : Level.On;
                iost.Origin = (status & (1u << 4)) == 0 ? Level.Off : Level.On;

                return FinSucc(iost);
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
                double pos = 0;
                CheckResult(LTSMC.smc_get_position_unit(CardNo, ToUShort(axis, nameof(axis)), ref pos));
                return FinSucc(pos);
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
                double pos = 0;
                CheckResult(LTSMC.smc_get_encoder_unit(CardNo, ToUShort(axis, nameof(axis)), ref pos));
                return FinSucc(pos);
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
                short state = LTSMC.smc_read_inbit(CardNo, ToUShort(bitNo, nameof(bitNo)));
                return FinSucc(state == 0 ? Level.Off : Level.On);
            }
            catch (Exception ex)
            {
                return FinFail<Level>(MotionError($"GetInput({bitNo})", ex));
            }
        }

        public Fin<int> GetOutput(TOut bitNo)
        {
            try
            {
                return FinSucc((int)LTSMC.smc_read_outbit(CardNo, ToUShort(bitNo, nameof(bitNo))));
            }
            catch (Exception ex)
            {
                return FinFail<int>(MotionError($"GetOutput({bitNo})", ex));
            }
        }

        public Fin<double> GetSpeed(TAxis axis)
        {
            try
            {
                double speed = 0;
                CheckResult(LTSMC.smc_read_current_speed_unit(CardNo, ToUShort(axis, nameof(axis)), ref speed));
                return FinSucc(speed);
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
                CheckResult(LTSMC.smc_home_move(CardNo, ToUShort(axis, nameof(axis))));
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
                CheckResult(LTSMC.smc_board_init(CardNo, 2, IP, 115200));
                InitDelegate?.Invoke(CardNo);
                _isConnected = true;
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
                // 1: 绝对模式
                CheckResult(LTSMC.smc_pmove_unit(CardNo, ToUShort(axis, nameof(axis)), pos, 1));
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
                CheckResult(LTSMC.smc_vmove(CardNo, ToUShort(axis, nameof(axis)), (ushort)dir));
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
                // 0: 相对模式
                CheckResult(LTSMC.smc_pmove_unit(CardNo, ToUShort(axis, nameof(axis)), pos, 0));
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
                CheckResult(LTSMC.smc_set_position_unit(CardNo, ToUShort(axis, nameof(axis)), pos));
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
                CheckResult(LTSMC.smc_set_encoder_unit(CardNo, ToUShort(axis, nameof(axis)), pos));
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
                CheckResult(LTSMC.smc_write_outbit(CardNo, ToUShort(bitNo, nameof(bitNo)), (ushort)level));
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
                ushort axisNo = ToUShort(axis, nameof(axis));
                CheckResult(LTSMC.smc_set_profile_unit(CardNo, axisNo, speed.Min, speed.Max, speed.Tacc, speed.Tdec, speed.Stop));
                CheckResult(LTSMC.smc_set_s_profile(CardNo, axisNo, 0, speed.S_Para));
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
                // 0:减速停止
                CheckResult(LTSMC.smc_stop(CardNo, ToUShort(axis, nameof(axis)), 0));
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
                // 1:立即停止
                CheckResult(LTSMC.smc_stop(CardNo, ToUShort(axis, nameof(axis)), 1));
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                return FinFail<LUnit>(MotionError($"EStop({axis})", ex));
            }
        }
    }
}
