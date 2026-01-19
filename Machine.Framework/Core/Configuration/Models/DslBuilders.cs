using System;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Core.Configuration.Models
{
    public class BoardBuilder
    {
        public string Name { get; }
        public ControlBoardConfig Config { get; }

        public BoardBuilder(string name)
        {
            Name = name;
            Config = new ControlBoardConfig(name);
        }

        public BoardBuilder MapAxis(Enum axis, int channel)
        {
            Config.AxisMappings[axis.ToString()] = channel;
            return this;
        }

        public BoardBuilder MapAxis(string axisId, int channel)
        {
            Config.AxisMappings[axisId] = channel;
            return this;
        }

        public BoardBuilder MapAxis(AxisID axis, int channel)
        {
            return MapAxis(axis.Name, channel);
        }

        public BoardBuilder MapCylinder(string cylinderId, int doOut)
        {
            Config.CylinderMappings[cylinderId] = new CylinderBinding(doOut);
            return this;
        }

        public BoardBuilder MapCylinder(CylinderID cylinder, int doOut)
        {
            return MapCylinder(cylinder.Name, doOut);
        }

        public BoardBuilder MapCylinder(string cylinderId, int doOut, int extendedPort, int retractedPort)
        {
            Config.CylinderMappings[cylinderId] = new CylinderBinding(doOut).WithFeedback(extendedPort, retractedPort);
            return this;
        }

        public BoardBuilder MapCylinder(CylinderID cylinder, int doOut, int extendedPort, int retractedPort)
        {
            return MapCylinder(cylinder.Name, doOut, extendedPort, retractedPort);
        }

        public BoardBuilder MapInput(Enum di, int port)
        {
            return MapInput(di.ToString(), port);
        }

        public BoardBuilder MapInput(string inputId, int port)
        {
            Config.InputMappings[inputId] = port;
            return this;
        }

        public BoardBuilder MapOutput(Enum doo, int port)
        {
            return MapOutput(doo.ToString(), port);
        }

        public BoardBuilder MapOutput(string outputId, int port)
        {
            Config.OutputMappings[outputId] = port;
            return this;
        }

        public BoardBuilder UseSimulator()
        {
            Config.Driver = new SimulatorDriverConfig();
            return this;
        }

        public BoardBuilder UseLeadshine()
        {
            Config.Driver = new LeadshineDriverConfig();
            return this;
        }

        public BoardBuilder UseZMotion()
        {
            Config.Driver = new ZMotionDriverConfig();
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
