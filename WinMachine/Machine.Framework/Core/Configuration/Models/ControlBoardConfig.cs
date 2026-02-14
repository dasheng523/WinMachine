using System;
using System.Collections.Generic;

namespace Machine.Framework.Core.Configuration.Models
{
    public sealed class ControlBoardConfig
    {
        public string Name { get; set; } = string.Empty;

        // 板级映射：逻辑设备名 -> 板通道/端口
        public Dictionary<string, int> AxisMappings { get; set; } = new();
        public Dictionary<string, CylinderBinding> CylinderMappings { get; set; } = new();
        public Dictionary<string, int> InputMappings { get; set; } = new();
        public Dictionary<string, int> OutputMappings { get; set; } = new();

        // 实现相关：驱动/板卡型号/连接参数/仿真细节等
        public BoardDriverConfig? Driver { get; set; }

        public ControlBoardConfig() { }

        public ControlBoardConfig(string name)
        {
            Name = name;
        }
    }

    public sealed class CylinderBinding
    {
        public int OutputPort { get; set; }
        public int? ExtendedSensorPort { get; set; }
        public int? RetractedSensorPort { get; set; }

        public CylinderBinding() { }

        public CylinderBinding(int outputPort)
        {
            OutputPort = outputPort;
        }

        public CylinderBinding WithFeedback(int extendedPort, int retractedPort)
        {
            ExtendedSensorPort = extendedPort;
            RetractedSensorPort = retractedPort;
            return this;
        }
    }
}
