using System;

namespace Machine.Framework.Core.Configuration.Models
{
    public class BoardBuilder
    {
        public string Name { get; }
        public BaseBoardConfig? Config { get; private set; }

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

        public BoardBuilder UseSimulator(Action<SimulatorBoardConfig> configure)
        {
            var config = new SimulatorBoardConfig(Name);
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
        public BaseDeviceConfig? Config { get; private set; }

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
}
