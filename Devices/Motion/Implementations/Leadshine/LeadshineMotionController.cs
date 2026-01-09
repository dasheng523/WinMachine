using System;
using System.Collections.Generic;
using Devices.Motion.Abstractions;
using Common.Core;
using Leadshine;

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

        public Action<ushort> InitDelegate { get; set; }

        private bool _isConnected;

        public LeadshineMotionController(string ip, ushort cardNo, Action<ushort> initDelegate = null)
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

        private static ushort ToUShort(object value, string paramName)
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

        public void AxisEnable(TAxis axis, Level enable)
        {
            CheckResult(LTSMC.smc_write_sevon_pin(CardNo, ToUShort(axis, nameof(axis)), (ushort)enable));
        }

        public bool CheckDone(TAxis axis)
        {
            // smc_check_done：0-运行中，1-停止
            return LTSMC.smc_check_done(CardNo, ToUShort(axis, nameof(axis))) == 1;
        }

        public bool CheckHomeDone(TAxis axis)
        {
            ushort state = 0;
            CheckResult(LTSMC.smc_get_home_result(CardNo, ToUShort(axis, nameof(axis)), ref state));
            return state == 1;
        }

        public void Dispose()
        {
            if (_isConnected)
            {
                LTSMC.smc_board_close(CardNo);
                _isConnected = false;
            }
        }

        public Level GetAxisAlarm(TAxis axis)
        {
            // smc_read_alarm_pin：一般返回 0/1
            short state = LTSMC.smc_read_alarm_pin(CardNo, ToUShort(axis, nameof(axis)));
            return state == 0 ? Level.Off : Level.On;
        }

        public AxisStatus GetAxisStatus(TAxis axis)
        {
            uint status = LTSMC.smc_axis_io_status(CardNo, ToUShort(axis, nameof(axis)));
            AxisStatus iost = new AxisStatus();

            // 雷赛 IO 状态位映射
            iost.ServoAlarm = (status & (1u << 0)) == 0 ? Level.Off : Level.On;
            iost.PositiveHardLimit = (status & (1u << 1)) == 0 ? Level.Off : Level.On;
            iost.NegativeHardLimit = (status & (1u << 2)) == 0 ? Level.Off : Level.On;
            iost.EmergencyStop = (status & (1u << 3)) == 0 ? Level.Off : Level.On;
            iost.Origin = (status & (1u << 4)) == 0 ? Level.Off : Level.On;

            return iost;
        }

        public double GetCommandPos(TAxis axis)
        {
            double pos = 0;
            CheckResult(LTSMC.smc_get_position_unit(CardNo, ToUShort(axis, nameof(axis)), ref pos));
            return pos;
        }

        public double GetEncoderPos(TAxis axis)
        {
            double pos = 0;
            CheckResult(LTSMC.smc_get_encoder_unit(CardNo, ToUShort(axis, nameof(axis)), ref pos));
            return pos;
        }

        public Level GetInput(TIn bitNo)
        {
            short state = LTSMC.smc_read_inbit(CardNo, ToUShort(bitNo, nameof(bitNo)));
            return state == 0 ? Level.Off : Level.On;
        }

        public int GetOutput(TOut bitNo)
        {
            return LTSMC.smc_read_outbit(CardNo, ToUShort(bitNo, nameof(bitNo)));
        }

        public double GetSpeed(TAxis axis)
        {
            double speed = 0;
            CheckResult(LTSMC.smc_read_current_speed_unit(CardNo, ToUShort(axis, nameof(axis)), ref speed));
            return speed;
        }

        public void GoBackHome(TAxis axis)
        {
            CheckResult(LTSMC.smc_home_move(CardNo, ToUShort(axis, nameof(axis))));
        }

        public void Initialization()
        {
            CheckResult(LTSMC.smc_board_init(CardNo, 2, IP, 115200));
            InitDelegate?.Invoke(CardNo);
            _isConnected = true;
        }

        public void Move_Absolute(TAxis axis, double pos)
        {
            // 1: 绝对模式
            CheckResult(LTSMC.smc_pmove_unit(CardNo, ToUShort(axis, nameof(axis)), pos, 1));
        }

        public void Move_JOG(TAxis axis, MotionDirection dir)
        {
            CheckResult(LTSMC.smc_vmove(CardNo, ToUShort(axis, nameof(axis)), (ushort)dir));
        }

        public void Move_Relative(TAxis axis, double pos)
        {
            // 0: 相对模式
            CheckResult(LTSMC.smc_pmove_unit(CardNo, ToUShort(axis, nameof(axis)), pos, 0));
        }

        public void SetCommandPos(TAxis axis, double pos)
        {
            CheckResult(LTSMC.smc_set_position_unit(CardNo, ToUShort(axis, nameof(axis)), pos));
        }

        public void SetEncoderPos(TAxis axis, double pos)
        {
            CheckResult(LTSMC.smc_set_encoder_unit(CardNo, ToUShort(axis, nameof(axis)), pos));
        }

        public void SetOutput(TOut bitNo, Level level)
        {
            CheckResult(LTSMC.smc_write_outbit(CardNo, ToUShort(bitNo, nameof(bitNo)), (ushort)level));
        }

        public void SetSpeed(TAxis axis, AxisSpeed speed)
        {
            ushort axisNo = ToUShort(axis, nameof(axis));
            CheckResult(LTSMC.smc_set_profile_unit(CardNo, axisNo, speed.Min, speed.Max, speed.Tacc, speed.Tdec, speed.Stop));
            CheckResult(LTSMC.smc_set_s_profile(CardNo, axisNo, 0, speed.S_Para));
        }

        public void Stop(TAxis axis)
        {
            // 0:减速停止
            CheckResult(LTSMC.smc_stop(CardNo, ToUShort(axis, nameof(axis)), 0));
        }

        public void EStop(TAxis axis)
        {
            // 1:立即停止
            CheckResult(LTSMC.smc_stop(CardNo, ToUShort(axis, nameof(axis)), 1));
        }
    }
}
