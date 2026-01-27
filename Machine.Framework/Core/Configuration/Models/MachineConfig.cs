using System;
using System.Collections.Generic;
using System.Text.Json;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Core.Configuration.Models
{
    public class MachineConfig
    {
        public List<ControlBoardConfig> BoardConfigs { get; set; } = new List<ControlBoardConfig>();
        public List<BaseDeviceConfig> DeviceConfigs { get; set; } = new List<BaseDeviceConfig>();
        public List<BusConfig> BusConfigs { get; set; } = new List<BusConfig>();
        public Dictionary<string, AxisConfig> AxisConfigs { get; set; } = new Dictionary<string, AxisConfig>();
        public Dictionary<string, CylinderConfig> CylinderConfigs { get; set; } = new Dictionary<string, CylinderConfig>();

        // Added MountPoints for Kinematics
        public List<MountPointDefinition> MountPoints { get; set; } = new List<MountPointDefinition>();
        
        internal static MachineConfig Create()
        {
            return new MachineConfig();
        }

        /// <summary>
        /// 仅限蓝图解释器使用的内部创建入口
        /// </summary>
        internal static MachineConfig Internal_Create_By_Blueprint()
        {
            return new MachineConfig();
        }

        public MachineConfig AddControlBoard(string name, Action<BoardBuilder> configure)
        {
            var builder = new BoardBuilder(name);
            configure(builder);
            BoardConfigs.Add(builder.Config);
            return this;
        }

        public MachineConfig ConfigureAxis(string axisId, Action<AxisConfigBuilder> configure)
        {
            var builder = new AxisConfigBuilder();
            configure(builder);
            AxisConfigs[axisId] = builder.Build();
            return this;
        }

        public MachineConfig ConfigureAxis(AxisID axis, Action<AxisConfigBuilder> configure)
        {
            return ConfigureAxis(axis.Name, configure);
        }

        public MachineConfig ConfigureAxis(Enum axis, Action<AxisConfigBuilder> configure)
        {
            return ConfigureAxis(axis.ToString(), configure);
        }

        public MachineConfig ConfigureCylinder(string cylinderId, Action<CylinderConfig> configure)
        {
            if (!CylinderConfigs.TryGetValue(cylinderId, out var cfg))
            {
                cfg = new CylinderConfig(cylinderId);
            }

            configure(cfg);
            CylinderConfigs[cylinderId] = cfg;
            return this;
        }

        public MachineConfig ConfigureCylinder(CylinderID cylinder, Action<CylinderConfig> configure)
        {
            return ConfigureCylinder(cylinder.Name, configure);
        }

        public MachineConfig UseSimulator(string boardName, Action<SimulatorDriverConfig> configure)
        {
            var board = GetBoardOrThrow(boardName);
            var cfg = new SimulatorDriverConfig();
            configure(cfg);
            board.Driver = cfg;
            return this;
        }

        public MachineConfig UseLeadshine(string boardName, Action<LeadshineDriverConfig> configure)
        {
            var board = GetBoardOrThrow(boardName);
            var cfg = new LeadshineDriverConfig();
            configure(cfg);
            board.Driver = cfg;
            return this;
        }

        public MachineConfig UseZMotion(string boardName, Action<ZMotionDriverConfig> configure)
        {
            var board = GetBoardOrThrow(boardName);
            var cfg = new ZMotionDriverConfig();
            configure(cfg);
            board.Driver = cfg;
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

        private ControlBoardConfig GetBoardOrThrow(string boardName)
        {
            var board = BoardConfigs.Find(b => string.Equals(b.Name, boardName, StringComparison.OrdinalIgnoreCase));
            if (board == null)
            {
                throw new InvalidOperationException($"Control board '{boardName}' not found. Did you forget AddControlBoard('{boardName}', ...)?");
            }

            return board;
        }
    }

    public record MountPointDefinition(
        string Name,
        string? ParentName,       
        object? LinkedDevice,     
        double OffsetX, double OffsetY, double OffsetZ,
        double RotationX, double RotationY, double RotationZ, // New: Initial rotation
        double StrokeX, double StrokeY, double StrokeZ,       // New: Actuation vector
        List<MountPointDefinition> Children
    );
}
