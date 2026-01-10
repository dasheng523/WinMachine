using System;
using System.Collections.Generic;
using System.Text;
using Common.Core;
using LanguageExt;
using LUnit = LanguageExt.Unit;

namespace Devices.Motion.Abstractions
{
    public interface IMotionController<TAxis, TIn, TOut> : IDisposable
    {
        /// <summary>
        /// 初始化运动控制卡/控制器连接。
        /// </summary>
        Fin<LUnit> Initialization();


        /// <summary>
        /// 读取轴速度。
        /// </summary>
        Fin<double> GetSpeed(TAxis axis);

        /// <summary>
        /// 设置轴速度
        /// </summary>
        Fin<LUnit> SetSpeed(TAxis axis, AxisSpeed speed);

        /// <summary>
        /// 轴停止。
        /// </summary>
        Fin<LUnit> Stop(TAxis axis);

        /// <summary>
        /// 轴急停。
        /// </summary>
        Fin<LUnit> EStop(TAxis axis);

        /// <summary>
        /// 绝对运动。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">位置</param>
        Fin<LUnit> Move_Absolute(TAxis axis, double pos);

        /// <summary>
        /// 相对运动。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">位置</param>
        Fin<LUnit> Move_Relative(TAxis axis, double pos);

        /// <summary>
        /// 连续运动 (Enum Overload)。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="dir">方向枚举</param>
        Fin<LUnit> Move_JOG(TAxis axis, MotionDirection dir);

        /// <summary>
        /// 检查轴运动状态。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>true-轴停止，false-轴运动中</returns>
        Fin<bool> CheckDone(TAxis axis);

        /// <summary>
        /// 回原点运动。
        /// </summary>
        /// <param name="axis">轴号</param>
        Fin<LUnit> GoBackHome(TAxis axis);

        /// <summary>
        /// 检查轴回原点状态。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>true-回原点完成，false-回原点失败</returns>
        Fin<bool> CheckHomeDone(TAxis axis);

        /// <summary>
        /// 读取轴指令脉冲位置。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>轴当前指令位置</returns>
        Fin<double> GetCommandPos(TAxis axis);

        /// <summary>
        /// 设置轴指令脉冲位置。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">位置</param>
        Fin<LUnit> SetCommandPos(TAxis axis, double pos);

        /// <summary>
        /// 读取轴编码器脉冲位置。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>轴当前编码器位置</returns>
        Fin<double> GetEncoderPos(TAxis axis);

        /// <summary>
        /// 设置轴编码器脉冲位置。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">位置</param>
        Fin<LUnit> SetEncoderPos(TAxis axis, double pos);

        /// <summary>
        /// 读取轴状态。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>轴状态结构体</returns>
        Fin<AxisStatus> GetAxisStatus(TAxis axis);

        /// <summary>
        /// 读取轴报警信号。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <returns>0-正常，1-异常</returns>
        Fin<Level> GetAxisAlarm(TAxis axis);

        /// <summary>
        /// 轴使能。
        /// </summary>
        /// <param name="axis">枚举</param>
        /// <param name="enable">枚举</param>
        Fin<LUnit> AxisEnable(TAxis axis, Level enable);

        /// <summary>
        /// 读取通用输出口状态。
        /// </summary>
        /// <param name="bitNo">输出口编号</param>
        /// <returns>0-低电平，1-高电平</returns>
        Fin<Level> GetOutput(TOut bitNo);


        /// <summary>
        /// 设置通用输出口状态。
        /// </summary>
        /// <param name="bitNo">输出口编号</param>
        /// <param name="level">电平</param>
        Fin<LUnit> SetOutput(TOut bitNo, Level level);

        /// <summary>
        /// 读取通用输入口状态。
        /// </summary>
        /// <param name="bitNo">输入口编号</param>
        /// <returns>0-低电平，1-高电平</returns>
        Fin<Level> GetInput(TIn bitNo);
    }
}
