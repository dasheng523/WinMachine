using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Machine.Framework.Configuration.Models
{
    // =========================================================================================
    // Builders
    // =========================================================================================

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

    // =========================================================================================
    // Configuration Models
    // =========================================================================================

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(LeadshineConfig), typeDiscriminator: "Leadshine")]
    [JsonDerivedType(typeof(ZMotionConfig), typeDiscriminator: "ZMotion")]
    public class BaseBoardConfig
    {
        public string Name { get; set; }
        public BaseBoardConfig(string name) { Name = name; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(SerialConfig), typeDiscriminator: "Serial")]
    [JsonDerivedType(typeof(TcpConfig), typeDiscriminator: "Tcp")]
    public abstract class BaseDeviceConfig
    {
        public string Name { get; set; }
        protected BaseDeviceConfig(string name) { Name = name; }
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

    // ===========================================
    // Axis Configuration Models & Builders
    // ===========================================

    // ===========================================
    // Axis Configuration Models & Builders
    // ===========================================

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

    public class AxisConfig
    {
        public PulseOutputMode? PulseOutput { get; set; }
        public EncoderInputMode? EncoderInput { get; set; }
        public EncoderDir? EncDirection { get; set; } // New
        public double? Equivalency { get; set; }
        public double? Backlash { get; set; }
        public HardLimitConfig? HardLimits { get; set; }
        public SoftLimitConfig? SoftLimits { get; set; }
        public AlarmConfig? Alarm { get; set; } // New
        public HomeConfig? Homing { get; set; }
        public List<AxisIoMapConfig> IoMappings { get; } = new List<AxisIoMapConfig>();
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

    public class SerialConfig : BaseDeviceConfig
    {
        // Name is in BaseDeviceConfig
        public string PortName { get; set; } = string.Empty;
        public int BaudRateVal { get; set; }
        public SerialProtocol ProtocolVal { get; set; }
        public Dictionary<string, int> FeatureMappings { get; set; } = new Dictionary<string, int>();

        public SerialConfig(string name) : base(name) { }

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
            FeatureMappings[feature.ToString()] = registerAddress;
            return this;
        }
    }

    public class TcpConfig : BaseDeviceConfig
    {
        // Name is in BaseDeviceConfig
        public string IpVal { get; set; } = string.Empty;
        public int PortVal { get; set; }

        public TcpConfig(string name) : base(name) { }

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
        public required string Name { get; set; }
        public SerialConfig? ProtocolConfig { get; set; }
        public List<BusNodeConfig> Nodes { get; } = new List<BusNodeConfig>();
    }

    public class BusNodeConfig
    {
        public string Name { get; set; }
        public int StationIdVal { get; set; }
        public Dictionary<string, int> FeatureMappings { get; set; } = new Dictionary<string, int>();

        public BusNodeConfig(string name) { Name = name; }

        public BusNodeConfig StationId(int id)
        {
            StationIdVal = id;
            return this;
        }

        public BusNodeConfig MapFeature(Enum feature, int registerAddress)
        {
            FeatureMappings[feature.ToString()] = registerAddress;
            return this;
        }
    }
}
