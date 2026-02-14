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

    // ------------------------------------------------------------------
    // 物理属性定义 (Physical Property Definitions)
    // ------------------------------------------------------------------
    
    /// <summary>
    /// 物理组件类型枚举。
    /// 定义了蓝图中挂载点可以声明的物理语义类型。
    /// </summary>
    public enum PhysicalType
    {
        /// <summary>未定义/默认</summary>
        None = 0,
        /// <summary>通用立方体（用于碰撞包围盒）</summary>
        Box,
        /// <summary>吸笔（圆柱体碰撞体）</summary>
        SuctionPen,
        /// <summary>夹爪</summary>
        Gripper,
        /// <summary>旋转台</summary>
        RotaryTable,
        /// <summary>直线导轨</summary>
        LinearGuide,
        /// <summary>物料槽/托盘</summary>
        MaterialSlot
    }

    /// <summary>
    /// 物理锚点枚举。
    /// 定义了物理组件的局部坐标原点位置。
    /// </summary>
    public enum PhysicalAnchor
    {
        /// <summary>几何中心</summary>
        Center = 0,
        /// <summary>底部中心（适用于垂直组件）</summary>
        BottomCenter,
        /// <summary>顶部中心（适用于垂直组件，如吸笔安装端）</summary>
        TopCenter,
        /// <summary>行程起点（适用于导轨，Stroke=0 处）</summary>
        StrokeStart,
        /// <summary>自定义偏移（需配合 CustomAnchorOffset 使用）</summary>
        Custom
    }

    /// <summary>
    /// 物理属性记录类。
    /// 封装了蓝图中挂载点的完整物理描述信息。
    /// </summary>
    public record PhysicalProperty(
        PhysicalType Type,
        double SizeX, double SizeY, double SizeZ,
        PhysicalAnchor Anchor,
        bool IsVertical,
        bool IsInverted,
        double Param1 = 0,  // 类型专属参数1（如吸笔直径、旋转台半径）
        double Param2 = 0   // 类型专属参数2（如吸笔长度）
    );

    /// <summary>
    /// 机械挂载点定义记录。
    /// 描述了机器的物理层级结构中的一个节点，包含位姿、运动和物理属性。
    /// </summary>
    public record MountPointDefinition(
        string Name,
        string? ParentName,       
        object? LinkedDevice,     
        double OffsetX, double OffsetY, double OffsetZ,
        double RotationX, double RotationY, double RotationZ,
        double StrokeX, double StrokeY, double StrokeZ,
        List<MountPointDefinition> Children,
        PhysicalProperty? Physical = null  // 新增：物理属性（可选）
    );
}
