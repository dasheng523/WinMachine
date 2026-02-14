using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Machine.Framework.Core.Configuration.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(SimulatorDriverConfig), typeDiscriminator: "Simulator")]
    [JsonDerivedType(typeof(LeadshineDriverConfig), typeDiscriminator: "Leadshine")]
    [JsonDerivedType(typeof(ZMotionDriverConfig), typeDiscriminator: "ZMotion")]
    public abstract class BoardDriverConfig
    {
        public string DriverType { get; protected set; } = string.Empty;
    }

    public sealed class SimulatorDriverConfig : BoardDriverConfig
    {
        public int TickMs { get; set; } = 16;

        public Dictionary<string, SimulatorAxisPhysicalConfig> Axes { get; set; } = new();

        public SimulatorDriverConfig()
        {
            DriverType = "Simulator";
        }

        public SimulatorDriverConfig Axis(string axisId, Action<SimulatorAxisPhysicalConfig> configure)
        {
            if (!Axes.TryGetValue(axisId, out var cfg))
            {
                cfg = new SimulatorAxisPhysicalConfig();
            }

            configure(cfg);
            Axes[axisId] = cfg;
            return this;
        }

        public SimulatorDriverConfig Timing(Action<SimulatorTimingConfig> configure)
        {
            var t = new SimulatorTimingConfig { TickMs = TickMs };
            configure(t);
            TickMs = t.TickMs;
            return this;
        }
    }

    public sealed class SimulatorTimingConfig
    {
        public int TickMs { get; set; } = 16;
    }

    public sealed class SimulatorAxisPhysicalConfig
    {
        public double? TravelMin { get; set; }
        public double? TravelMax { get; set; }

        public string? HomeSensor { get; set; }
        public string? NegLimitSensor { get; set; }
        public string? PosLimitSensor { get; set; }

        public SimulatorAxisPhysicalConfig Travel(double min, double max)
        {
            TravelMin = min;
            TravelMax = max;
            return this;
        }

        public SimulatorAxisPhysicalConfig HomeSensorId(string di)
        {
            HomeSensor = di;
            return this;
        }

        public SimulatorAxisPhysicalConfig LimitSensors(string negDi, string posDi)
        {
            NegLimitSensor = negDi;
            PosLimitSensor = posDi;
            return this;
        }
    }

    public sealed class LeadshineDriverConfig : BoardDriverConfig
    {
        public LeadshineModel ModelType { get; set; } = LeadshineModel.Unknown;
        public int BoardId { get; set; }
        public PulseOutputMode PulseModeVal { get; set; }

        // 实现相关的轴配置（厂商能力）
        public Dictionary<string, AxisConfig> AxisConfigs { get; set; } = new();

        public LeadshineDriverConfig()
        {
            DriverType = "Leadshine";
        }

        public LeadshineDriverConfig Model(LeadshineModel model)
        {
            ModelType = model;
            return this;
        }

        public LeadshineDriverConfig CardId(int id)
        {
            BoardId = id;
            return this;
        }

        public LeadshineDriverConfig PulseMode(PulseOutputMode mode)
        {
            PulseModeVal = mode;
            return this;
        }

        public LeadshineDriverConfig ConfigAxis(Enum axis, Action<AxisConfigBuilder> configure)
        {
            var builder = new AxisConfigBuilder();
            configure(builder);
            AxisConfigs[axis.ToString()] = builder.Build();
            return this;
        }

        public LeadshineDriverConfig ConfigAxis(string axisId, Action<AxisConfigBuilder> configure)
        {
            var builder = new AxisConfigBuilder();
            configure(builder);
            AxisConfigs[axisId] = builder.Build();
            return this;
        }
    }

    public sealed class ZMotionDriverConfig : BoardDriverConfig
    {
        public ZMotionModel ModelType { get; set; } = ZMotionModel.Unknown;
        public string Ip { get; set; } = string.Empty;

        public ZMotionDriverConfig()
        {
            DriverType = "ZMotion";
        }

        public ZMotionDriverConfig Model(ZMotionModel model)
        {
            ModelType = model;
            return this;
        }

        public ZMotionDriverConfig IpAddress(string ip)
        {
            Ip = ip;
            return this;
        }
    }
}
