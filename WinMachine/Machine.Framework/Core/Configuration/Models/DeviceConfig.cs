using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Machine.Framework.Core.Configuration.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(SerialConfig), typeDiscriminator: "Serial")]
    [JsonDerivedType(typeof(TcpConfig), typeDiscriminator: "Tcp")]
    public abstract class BaseDeviceConfig
    {
        public string Name { get; set; }
        protected BaseDeviceConfig(string name) { Name = name; }
    }

    public class SerialConfig : BaseDeviceConfig
    {
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
