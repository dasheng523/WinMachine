using System;
using System.Collections.Generic;

namespace Machine.Framework.Devices.Configuration
{
    public class MachineConfig
    {
        public List<object> BoardConfigs { get; private set; } = new List<object>();
        public List<object> DeviceConfigs { get; private set; } = new List<object>();
        public List<object> BusConfigs { get; private set; } = new List<object>();

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
