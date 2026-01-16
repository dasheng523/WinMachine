using System;
using System.Collections.Generic;

namespace Machine.Framework.Devices.Configuration
{
    // =========================================================================================
    // Builders
    // =========================================================================================

    public class BoardBuilder
    {
        public string Name { get; }
        public object? Config { get; private set; }

        public BoardBuilder(string name)
        {
            Name = name;
        }

        public BoardBuilder UseLeadshine(Action<LeadshineConfig> configure)
        {
            var config = new LeadshineConfig(Name);
            configure(config);
            Config = config;
            return this;
        }

        public BoardBuilder UseZMotion(Action<ZMotionConfig> configure)
        {
            var config = new ZMotionConfig(Name);
            configure(config);
            Config = config;
            return this;
        }
    }

    public class DeviceBuilder
    {
        public string Name { get; }
        public object? Config { get; private set; }

        public DeviceBuilder(string name)
        {
            Name = name;
        }

        public DeviceBuilder UseSerialDevice(Action<SerialConfig> configure)
        {
            var config = new SerialConfig(Name);
            configure(config);
            Config = config;
            return this;
        }

        public DeviceBuilder UseTcpDevice(Action<TcpConfig> configure)
        {
            var config = new TcpConfig(Name);
            configure(config);
            Config = config;
            return this;
        }
    }

    public class BusBuilder
    {
        public string Name { get; }
        public BusConfig Config { get; private set; }

        public BusBuilder(string name)
        {
            Name = name;
            Config = new BusConfig { Name = name };
        }

        public BusBuilder UseSerial(Action<SerialConfig> configure)
        {
            var serialConfig = new SerialConfig(Name); // Bus serial config usually doesn't need feature mapping
            configure(serialConfig);
            Config.ProtocolConfig = serialConfig;
            return this;
        }

        public BusBuilder MountDevice(string deviceName, Action<BusNodeConfig> configure)
        {
            var nodeConfig = new BusNodeConfig(deviceName);
            configure(nodeConfig);
            Config.Nodes.Add(nodeConfig);
            return this;
        }
    }

    // =========================================================================================
    // Configuration Models
    // =========================================================================================

    public class BaseBoardConfig
    {
        public string Name { get; set; }
        public BaseBoardConfig(string name) { Name = name; }
    }

    public class LeadshineConfig : BaseBoardConfig
    {
        public LeadshineModel ModelType { get; set; }
        public int BoardId { get; set; }
        public PulseMode PulseModeVal { get; set; }
        public Dictionary<Enum, int> AxisMappings { get; } = new Dictionary<Enum, int>();
        public Dictionary<Enum, int> InputMappings { get; } = new Dictionary<Enum, int>();
        public Dictionary<Enum, int> OutputMappings { get; } = new Dictionary<Enum, int>();

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

        public LeadshineConfig PulseMode(PulseMode mode)
        {
            this.PulseModeVal = mode;
            return this;
        }

        public LeadshineConfig MapAxis(Enum axis, int physicalIndex)
        {
            AxisMappings[axis] = physicalIndex;
            return this;
        }

        public LeadshineConfig MapInput(Enum di, int port)
        {
            InputMappings[di] = port;
            return this;
        }

        public LeadshineConfig MapOutput(Enum doo, int port)
        {
            OutputMappings[doo] = port;
            return this;
        }
    }

    public class ZMotionConfig : BaseBoardConfig
    {
        public ZMotionModel ModelType { get; set; }
        public string Ip { get; set; }
        public Dictionary<Enum, int> AxisMappings { get; } = new Dictionary<Enum, int>();

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
            AxisMappings[axis] = physicalIndex;
            return this;
        }
    }

    public class SerialConfig
    {
        public string Name { get; set; }
        public string PortName { get; set; }
        public int BaudRateVal { get; set; }
        public SerialProtocol ProtocolVal { get; set; }
        public Dictionary<Enum, int> FeatureMappings { get; } = new Dictionary<Enum, int>();

        public SerialConfig(string name) { Name = name; }

        public SerialConfig Port(string port)
        {
            PortName = port;
            return this;
        }

        public SerialConfig BaudRate(int rate)
        {
            BaudRateVal = rate;
            return this;
        }

        public SerialConfig Protocol(SerialProtocol protocol)
        {
            ProtocolVal = protocol;
            return this;
        }

        public SerialConfig MapFeature(Enum feature, int registerAddress)
        {
            FeatureMappings[feature] = registerAddress;
            return this;
        }
    }

    public class TcpConfig
    {
        public string Name { get; set; }
        public string IpVal { get; set; }
        public int PortVal { get; set; }

        public TcpConfig(string name) { Name = name; }

        public TcpConfig Ip(string ip)
        {
            IpVal = ip;
            return this;
        }

        public TcpConfig Port(int port)
        {
            PortVal = port;
            return this;
        }
    }

    public class BusConfig
    {
        public string Name { get; set; }
        public SerialConfig ProtocolConfig { get; set; }
        public List<BusNodeConfig> Nodes { get; } = new List<BusNodeConfig>();
    }

    public class BusNodeConfig
    {
        public string Name { get; set; }
        public int StationIdVal { get; set; }
        public Dictionary<Enum, int> FeatureMappings { get; } = new Dictionary<Enum, int>();

        public BusNodeConfig(string name) { Name = name; }

        public BusNodeConfig StationId(int id)
        {
            StationIdVal = id;
            return this;
        }

        public BusNodeConfig MapFeature(Enum feature, int registerAddress)
        {
            FeatureMappings[feature] = registerAddress;
            return this;
        }
    }
}
