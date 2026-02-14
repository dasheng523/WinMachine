using System;

namespace Machine.Framework.Core.Configuration.Models
{
    /// <summary>
    /// 气缸硬件配置，定义其 IO 绑定关系。
    /// 支持物理端口号 (Port) 或 逻辑名称 (Logic Name)。
    /// </summary>
    public class CylinderConfig
    {
        public string Name { get; set; } = string.Empty;

        // 控制输出 (支持逻辑 ID 或 物理端口)
        public string MoveDo { get; set; } = string.Empty;
        public int OutputPort { get; set; }

        // 反馈输入（可选）
        public string ExtendedDi { get; set; } = string.Empty;
        public string RetractedDi { get; set; } = string.Empty;
        
        public int? ExtendedSensorPort { get; set; }
        public int? RetractedSensorPort { get; set; }

        // 默认超时时间 (ms)
        public int DefaultTimeoutMs { get; set; } = 2000;
        public int MoveTime { get; set; } = 1000; // 兼容旧属性

        public CylinderConfig() { }
        public CylinderConfig(string name) => Name = name;

        public CylinderConfig Drive(int port)
        {
            OutputPort = port;
            return this;
        }

        public CylinderConfig Drive(string logicName)
        {
            MoveDo = logicName;
            return this;
        }

        public CylinderConfig WithSensors(int extendedPort, int retractedPort)
        {
            ExtendedSensorPort = extendedPort;
            RetractedSensorPort = retractedPort;
            return this;
        }

        public CylinderConfig WithSensors(string extendedDi, string retractedDi)
        {
            ExtendedDi = extendedDi;
            RetractedDi = retractedDi;
            return this;
        }
    }
}
