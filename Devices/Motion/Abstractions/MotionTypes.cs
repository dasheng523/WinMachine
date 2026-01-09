using Common.Core;
using System;

namespace Devices.Motion.Abstractions
{
    /// <summary>
    /// 运动方向
    /// </summary>
    public enum MotionDirection
    {
        Positive = 0,
        Negative = 1
    }

    /// <summary>
    /// 轴速度参数
    /// </summary>
    public struct AxisSpeed
    {
        public double Min;              //起始速度           
        public double Max;              //目标速度
        public double Tacc;             //加速时间
        public double Tdec;             //减速时间
        public double Stop;             //停止速度
        public double S_Para;           //平滑时间

        public AxisSpeed(double min, double max, double tacc, double tdec, double stop, double s_para)
        {
            Min = min;
            Max = max;
            Tacc = tacc;
            Tdec = tdec;
            Stop = stop;
            S_Para = s_para;
        }
    }

    /// <summary>
    /// 轴状态详细信息
    /// </summary>
    public struct AxisStatus
    {
        public Level ServoAlarm;               // 伺服报警信号 ALM
        public Level PositiveHardLimit;        // 正硬限位信号 EL+
        public Level NegativeHardLimit;        // 负硬限位信号 EL-
        public Level EmergencyStop;            // 急停信号     EMG
        public Level Origin;                   // 原点信号     ORG
    }

}