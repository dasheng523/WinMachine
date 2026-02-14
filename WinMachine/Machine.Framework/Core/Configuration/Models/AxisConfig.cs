using System;
using System.Collections.Generic;

namespace Machine.Framework.Core.Configuration.Models
{
    public class AxisConfig
    {
        public PulseOutputMode? PulseOutput { get; set; }
        public EncoderInputMode? EncoderInput { get; set; }
        public EncoderDir? EncDirection { get; set; }
        public double? Equivalency { get; set; }
        public double? Backlash { get; set; }
        public HardLimitConfig? HardLimits { get; set; }
        public SoftLimitConfig? SoftLimits { get; set; }
        public AlarmConfig? Alarm { get; set; }
        public HomeConfig? Homing { get; set; }
        public List<AxisIoMapConfig> IoMappings { get; } = new List<AxisIoMapConfig>();
    }

    public class AxisConfigBuilder
    {
        private AxisConfig _config = new AxisConfig();

        public AxisConfig Build() => _config;

        public AxisConfigBuilder SetPulseOutput(PulseOutputMode mode)
        {
            _config.PulseOutput = mode;
            return this;
        }

        public AxisConfigBuilder SetEncInput(EncoderInputMode mode)
        {
            _config.EncoderInput = mode;
            return this;
        }

        public AxisConfigBuilder SetEquivalency(double pulsePerUnit)
        {
            _config.Equivalency = pulsePerUnit;
            return this;
        }

        public AxisConfigBuilder SetBacklash(double val)
        {
            _config.Backlash = val;
            return this;
        }

        public AxisConfigBuilder SetHardLimits(Action<HardLimitConfig> action)
        {
            _config.HardLimits = new HardLimitConfig();
            action(_config.HardLimits);
            return this;
        }

        public AxisConfigBuilder SetSoftLimits(Action<SoftLimitConfig> action)
        {
            _config.SoftLimits = new SoftLimitConfig();
            action(_config.SoftLimits);
            return this;
        }

        public AxisConfigBuilder SetHoming(Action<HomeConfig> action)
        {
            _config.Homing = new HomeConfig();
            action(_config.Homing);
            return this;
        }

        public AxisConfigBuilder SetAlarmConfig(Action<AlarmConfig> action)
        {
            _config.Alarm = new AlarmConfig();
            action(_config.Alarm);
            return this;
        }

        public AxisConfigBuilder SetEncDirection(EncoderDir dir)
        {
            _config.EncDirection = dir;
            return this;
        }

        public AxisConfigBuilder MapAxisIo(AxisIoType type, IoMapType targetType, int index, double filterTime)
        {
            _config.IoMappings.Add(new AxisIoMapConfig 
            { 
                IoType = type, 
                MapTargetType = targetType, 
                MapIndex = index, 
                FilterTime = filterTime 
            });
            return this;
        }
    }

    public class HardLimitConfig
    {
        public bool IsEnabled { get; set; } = true;
        public ActiveLevel LogicLevel { get; set; }
        public StopAction StopActionVal { get; set; }

        public HardLimitConfig Enable(bool enable)
        {
            IsEnabled = enable;
            return this;
        }

        public HardLimitConfig Logic(ActiveLevel level)
        {
            LogicLevel = level;
            return this;
        }

        public HardLimitConfig StopMode(StopAction action)
        {
            this.StopActionVal = action;
            return this;
        }
    }

    public class SoftLimitConfig
    {
        public bool IsEnabled { get; set; } = true;
        public double Min { get; set; }
        public double Max { get; set; }
        public StopAction StopActionVal { get; set; }

        public SoftLimitConfig Enable(bool enable)
        {
            IsEnabled = enable;
            return this;
        }

        public SoftLimitConfig Range(double min, double max)
        {
            Min = min;
            Max = max;
            return this;
        }

        public SoftLimitConfig Action(StopAction action)
        {
            StopActionVal = action;
            return this;
        }
    }

    public class AlarmConfig
    {
        public bool IsEnabled { get; set; } = true;
        public ActiveLevel LogicLevel { get; set; }

        public AlarmConfig Enable(bool enable)
        {
            IsEnabled = enable;
            return this;
        }

        public AlarmConfig Logic(ActiveLevel level)
        {
            LogicLevel = level;
            return this;
        }
    }

    public class HomeConfig
    {
        public HomeMode ModeVal { get; set; }
        public HomeDir DirectionVal { get; set; } = HomeDir.Positive; // Default
        public double HighSpeedVal { get; set; }
        public double LowSpeedVal { get; set; }
        public double AccDecVal { get; set; } // Acceleration/Deceleration
        public ActiveLevel OrgLogicLevel { get; set; }

        public HomeConfig Mode(HomeMode mode)
        {
            ModeVal = mode;
            return this;
        }

        public HomeConfig Direction(HomeDir dir)
        {
            DirectionVal = dir;
            return this;
        }

        public HomeConfig HighSpeed(double speed)
        {
            HighSpeedVal = speed;
            return this;
        }

        public HomeConfig LowSpeed(double speed)
        {
            LowSpeedVal = speed;
            return this;
        }

        public HomeConfig Acceleration(double acc)
        {
            AccDecVal = acc;
            return this;
        }

        public HomeConfig OrgLogic(ActiveLevel level)
        {
            OrgLogicLevel = level;
            return this;
        }
    }

    public class AxisIoMapConfig
    {
        public AxisIoType IoType { get; set; }
        public IoMapType MapTargetType { get; set; }
        public int MapIndex { get; set; }
        public double FilterTime { get; set; }
    }
}
