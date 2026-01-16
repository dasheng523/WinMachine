using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Machine.Framework.Configuration.Models
{
    public class MachineConfig
    {
        public List<BaseBoardConfig> BoardConfigs { get; set; } = new List<BaseBoardConfig>();
        public List<BaseDeviceConfig> DeviceConfigs { get; set; } = new List<BaseDeviceConfig>();
        public List<BusConfig> BusConfigs { get; set; } = new List<BusConfig>();

        public static MachineConfig Create()
        {
            return new MachineConfig();
        }

        public MachineConfig AddControlBoard(string name, Action<BoardBuilder> configure)
        {
            var builder = new BoardBuilder(name);
            configure(builder);
            if (builder.Config != null)
            {
                BoardConfigs.Add(builder.Config);
            }
            return this;
        }

        public MachineConfig AddDevice(string name, Action<DeviceBuilder> configure)
        {
            var builder = new DeviceBuilder(name);
            configure(builder);
            if (builder.Config != null)
            {
                DeviceConfigs.Add(builder.Config);
            }
            return this;
        }

        public MachineConfig AddBus(string name, Action<BusBuilder> configure)
        {
            var builder = new BusBuilder(name);
            configure(builder);
            if (builder.Config != null)
            {
                BusConfigs.Add(builder.Config);
            }
            return this;
        }

    }
}
