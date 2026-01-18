using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Machine.Framework.Core.Configuration.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(LeadshineConfig), typeDiscriminator: "Leadshine")]
    [JsonDerivedType(typeof(ZMotionConfig), typeDiscriminator: "ZMotion")]
    [JsonDerivedType(typeof(SimulatorBoardConfig), typeDiscriminator: "Simulator")]
    public class BaseBoardConfig
    {
        public string Name { get; set; }
        public BaseBoardConfig(string name) { Name = name; }
    }

    /// <summary>
    /// 纯仿真板卡配置
    /// </summary>
    public class SimulatorBoardConfig : BaseBoardConfig
    {
        public Dictionary<string, int> AxisMappings { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, AxisConfig> AxisConfigs { get; set; } = new Dictionary<string, AxisConfig>();

        public SimulatorBoardConfig(string name) : base(name) { }

        public SimulatorBoardConfig MapAxis(Enum axis, int physicalIndex)
        {
            AxisMappings[axis.ToString()] = physicalIndex;
            return this;
        }

        public SimulatorBoardConfig MapAxis(string axisId, int physicalIndex)
        {
            AxisMappings[axisId] = physicalIndex;
            return this;
        }

        public SimulatorBoardConfig ConfigAxis(Enum axis, Action<AxisConfigBuilder> configure)
        {
            var builder = new AxisConfigBuilder();
            configure(builder);
            AxisConfigs[axis.ToString()] = builder.Build();
            return this;
        }

        public SimulatorBoardConfig ConfigAxis(string axisId, Action<AxisConfigBuilder> configure)
        {
            var builder = new AxisConfigBuilder();
            configure(builder);
            AxisConfigs[axisId] = builder.Build();
            return this;
        }
    }

    public class LeadshineConfig : BaseBoardConfig
    {
        public LeadshineModel ModelType { get; set; }
        public int BoardId { get; set; }
        public PulseOutputMode PulseModeVal { get; set; }
        public Dictionary<string, int> AxisMappings { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> InputMappings { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> OutputMappings { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, AxisConfig> AxisConfigs { get; set; } = new Dictionary<string, AxisConfig>();

        public LeadshineConfig(string name) : base(name) { }

        public LeadshineConfig Model(LeadshineModel model)
        {
            ModelType = model;
            return this;
        }

        public LeadshineConfig CardId(int id)
        {
            BoardId = id;
            return this;
        }

        public LeadshineConfig PulseMode(PulseOutputMode mode)
        {
            this.PulseModeVal = mode;
            return this;
        }

        public LeadshineConfig MapAxis(Enum axis, int physicalIndex)
        {
            AxisMappings[axis.ToString()] = physicalIndex;
            return this;
        }

        public LeadshineConfig MapInput(Enum di, int port)
        {
            InputMappings[di.ToString()] = port;
            return this;
        }

        public LeadshineConfig MapOutput(Enum doo, int port)
        {
            OutputMappings[doo.ToString()] = port;
            return this;
        }

        public LeadshineConfig ConfigAxis(Enum axis, Action<AxisConfigBuilder> configure)
        {
            var builder = new AxisConfigBuilder();
            configure(builder);
            AxisConfigs[axis.ToString()] = builder.Build();
            return this;
        }
    }

    public class ZMotionConfig : BaseBoardConfig
    {
        public ZMotionModel ModelType { get; set; }
        public string Ip { get; set; } = string.Empty;
        public Dictionary<string, int> AxisMappings { get; set; } = new Dictionary<string, int>();

        public ZMotionConfig(string name) : base(name) { }

        public ZMotionConfig Model(ZMotionModel model)
        {
            ModelType = model;
            return this;
        }

        public ZMotionConfig IpAddress(string ip)
        {
            Ip = ip;
            return this;
        }

        public ZMotionConfig SelectAxis(Enum axis, int physicalIndex)
        {
            AxisMappings[axis.ToString()] = physicalIndex;
            return this;
        }
    }
}
