using System;
using System.Collections.Generic;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Configuration.Models;

namespace Machine.Framework.Core.Blueprint.Builders
{
    internal sealed class BlueprintAssembly
    {
        public string Name { get; }
        public List<BlueprintBoardDefinition> Boards { get; } = new();
        public List<(string Name, Action<DeviceBuilder> Config)> Devices { get; } = new();
        public List<(string Name, Action<BusBuilder> Config)> Buses { get; } = new();
        
        // 核心扩展：存储机械层级结构
        public List<MountPointDefinition> MountPoints { get; } = new();

        public BlueprintAssembly(string name) => Name = name;
    }

    // ------------------------------------

    // ------------------------------------

    internal sealed class MachineBuilder : IMachineBlueprintBuilder
    {
        public string Name { get; }
        internal BlueprintAssembly Assembly { get; }
        public MachineBuilder(string name) { Name = name; Assembly = new BlueprintAssembly(name); }

        public IBoardBuilder AddBoard(string name, int cardId)
        {
            var b = new BoardBuilder(name, cardId);
            Assembly.Boards.Add(new BlueprintBoardDefinition(name, cardId, b));
            return b;
        }

        public IMachineBlueprintBuilder AddBoard(string name, int cardId, Action<IBoardBuilder> config)
        {
            config(AddBoard(name, cardId));
            return this;
        }

        public IMachineBlueprintBuilder AddDevice(string name, Action<DeviceBuilder> configure)
        {
            Assembly.Devices.Add((name, configure));
            return this;
        }

        public IMachineBlueprintBuilder AddBus(string name, Action<BusBuilder> configure)
        {
            Assembly.Buses.Add((name, configure));
            return this;
        }

        // 根级挂载点
        public IMountPointBuilder Mount(string name)
        {
            var mp = new MountPointBuilder(name, null, Assembly);
            return mp; // Root mount point
        }

        public IMachineBlueprintBuilder Mount(string name, Action<IMountPointBuilder> config) 
        { 
            config(Mount(name)); 
            return this; 
        }

        public IMachineBlueprintBuilder Select(Func<IMachineBlueprintBuilder, IMachineBlueprintBuilder> s) => s(this);
        public TResult SelectMany<TIntermediate, TResult>(Func<IMachineBlueprintBuilder, TIntermediate> i, Func<IMachineBlueprintBuilder, TIntermediate, TResult> r)
            => r(this, i(this));
    }

    internal sealed class BoardBuilder : IBoardBuilder
    {
        public string Name { get; }
        public int CardId { get; }
        public DriverType DriverTypeValue { get; private set; } = DriverType.Simulator;
        public LeadshineDriverBuilder? LeadshineConfig { get; private set; }
        public ZMotionDriverBuilder? ZMotionConfig { get; private set; }
        
        public List<(string Name, int Port)> InputMappings = new();
        public List<(string Name, int Port)> OutputMappings = new();
        public List<BlueprintAxisDefinition> Axes = new();
        public List<BlueprintCylinderDefinition> Cylinders = new();

        public BoardBuilder(string n, int id) { Name = n; CardId = id; }
        public IBoardBuilder UseSimulator() { DriverTypeValue = DriverType.Simulator; return this; }
        public IBoardBuilder UseLeadshine(Action<ILeadshineDriverBuilder> cfg) { DriverTypeValue = DriverType.Leadshine; LeadshineConfig = new(); cfg(LeadshineConfig); return this; }
        public IBoardBuilder UseZMotion(Action<IZMotionDriverBuilder> cfg) { DriverTypeValue = DriverType.ZMotion; ZMotionConfig = new(); cfg(ZMotionConfig); return this; }

        public IBoardBuilder MapInput(Enum i, int p) { InputMappings.Add((i.ToString(), p)); return this; }
        public IBoardBuilder MapOutput(Enum o, int p) { OutputMappings.Add((o.ToString(), p)); return this; }

        public IBoardBuilder AddAxis(AxisID axis, int channel) { 
            var b = new AxisBuilder(channel, axis.Name, this); 
            b.Commit(); 
            return this; 
        }
        public IBoardBuilder AddAxis(AxisID axis, int channel, Action<IAxisBuilder> cfg) { 
            var b = new AxisBuilder(channel, axis.Name, this); 
            // Note: The previous logic for cfg(AddAxis(...)) was relying on AddAxis returning IAxisBuilder.
            // But now AddAxis returns IBoardBuilder. So we must instantiate builder, apply cfg, commit, then return this.
            
            // Wait, AddAxis(..., Action) expects cfg(IAxisBuilder). 
            // So we can't call this.AddAxis(axis, channel).
            // We must instantiate AxisBuilder directly here.
            
            cfg(b); // Apply config
            b.Commit();
            return this; 
        }

        public IBoardBuilder AddCylinder(CylinderID cyl, int o) { 
            var b = new CylinderBuilder(cyl.Name, o, null, this); 
            b.Commit(); 
            return this; 
        }

        public IBoardBuilder AddCylinder(CylinderID cyl, int o, int diOut, int diIn) { 
            var b = new CylinderBuilder(cyl.Name, o, null, this); 
            b.WithFeedback(diOut, diIn);
            b.Commit(); 
            return this; 
        }

        public IBoardBuilder AddCylinder(CylinderID cyl, int o, Action<ICylinderBuilder> cfg) { 
            var b = new CylinderBuilder(cyl.Name, o, null, this);
            cfg(b);
            b.Commit();
            return this; 
        }
    }

    internal sealed class AxisBuilder : IAxisBuilder
    {
        private readonly int _channel; private readonly string _name; private readonly BoardBuilder _board;
        private double _min = 0, _max = 1000, _v = 200, _a = 200; private bool _rev = false, _vert = false;
        public AxisBuilder(int ch, string n, BoardBuilder b) { _channel = ch; _name = n; _board = b; }
        public IAxisBuilder WithKinematics(double v, double a) { _v = v; _a = a; return this; }
        public IAxisBuilder WithRange(double min, double max) { _min = min; _max = max; return this; }
        public IAxisBuilder Vertical() { _vert = true; return this; }
        public IAxisBuilder Horizontal() { _vert = false; return this; }
        public IAxisBuilder Reversed() { _rev = true; return this; }
        public void Commit() { if (!_board.Axes.Exists(a => a.Name == _name)) _board.Axes.Add(new BlueprintAxisDefinition(_channel, _name, _min, _max, _v, _a, _rev, _vert)); }
    }

    internal sealed class CylinderBuilder : ICylinderBuilder
    {
        private readonly string _name; 
        private readonly int _o; 
        private int? _i; 
        private readonly BoardBuilder _board;
        private int? _fo, _fi; 
        private int _t = 200; 
        private bool _vert = false;

        public CylinderBuilder(string n, int o, int? i, BoardBuilder b) 
        { 
            _name = n; _o = o; _i = i; _board = b; 
        }

        // 允许在配置中设置缩回控制端口（用于双电控气缸）
        public ICylinderBuilder WithRetractPort(int port) { _i = port; return this; }

        public ICylinderBuilder WithFeedback(int o, int i) { _fo = o; _fi = i; return this; }
        public ICylinderBuilder WithDynamics(int ms) { _t = ms; return this; }
        public ICylinderBuilder Vertical() { _vert = true; return this; }
        public ICylinderBuilder Horizontal() { _vert = false; return this; }
        public void Commit() { if (!_board.Cylinders.Exists(c => c.Name == _name)) _board.Cylinders.Add(new BlueprintCylinderDefinition(_name, _o, _i, _fo, _fi, _t, _vert)); }
    }

    internal class MountPointBuilder : IMountPointBuilder
    {
        private readonly string _name;
        private readonly MountPointBuilder? _parent;
        private readonly BlueprintAssembly? _asm;
        
        private object? _linkedDevice;

        private double _ox, _oy, _oz; // Offset (Position)
        private double _rx, _ry, _rz; // Rotation (Euler)
        private double _sx, _sy, _sz; // Stroke Vector

        // ------------------------------------------------------------------
        // 物理属性状态字段 (Physical Property State)
        // ------------------------------------------------------------------
        private PhysicalType _physicalType = PhysicalType.None;
        private double _physicalSizeX, _physicalSizeY, _physicalSizeZ;
        private double _physicalParam1, _physicalParam2;
        private PhysicalAnchor _physicalAnchor = PhysicalAnchor.Center;
        private bool _isVertical;
        private bool _isInverted;
        
        // 存储子 Builder（而非定义），以支持链式调用时的延迟求值
        private readonly List<MountPointBuilder> _childBuilders = new();

        public MountPointBuilder(string name, MountPointBuilder? parent, BlueprintAssembly? asm)
        {
            _name = name;
            _parent = parent;
            _asm = asm;
            
            // 如果有父节点，则自动注册到父节点的子列表中
            _parent?._childBuilders.Add(this);
        }

        public IMountPointBuilder AttachedTo(object p) 
        { 
            return this; 
        }

        public IMountPointBuilder LinkTo(object a) 
        { 
            _linkedDevice = a; 
            PropagateUpdate();
            return this; 
        }
        
        public IMountPointBuilder WithTransform(Func<double, double> t) => this;

        public IMountPointBuilder WithOffset(double x, double y, double z) 
        { 
            _ox = x; _oy = y; _oz = z;
            PropagateUpdate();
            return this; 
        }

        public IMountPointBuilder AtPose(double x, double y, double z) => WithOffset(x, y, z);

        public IMountPointBuilder WithRotation(double x, double y, double z)
        {
            _rx = x; _ry = y; _rz = z;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder WithStroke(double x, double y, double z)
        {
            _sx = x; _sy = y; _sz = z;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder Mount(string n) 
        { 
            // 创建子 Builder，它会在构造函数中自动注册到本节点的 _childBuilders
            var child = new MountPointBuilder(n, this, null);
            PropagateUpdate();
            return child; 
        }
        
        public IMountPointBuilder Mount(string n, Action<IMountPointBuilder> c) 
        {
            var childBuilder = new MountPointBuilder(n, this, null);
            c(childBuilder);
            PropagateUpdate();
            return this; 
        }

        // ------------------------------------------------------------------
        // 物理属性 DSL 实现 (Physical Property DSL Implementation)
        // ------------------------------------------------------------------

        public IMountPointBuilder AsBox(double width, double height, double depth)
        {
            _physicalType = PhysicalType.Box;
            _physicalSizeX = width;
            _physicalSizeY = height;
            _physicalSizeZ = depth;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder AsSuctionPen(double diameter, double length)
        {
            _physicalType = PhysicalType.SuctionPen;
            _physicalParam1 = diameter;
            _physicalParam2 = length;
            // 吸笔：圆柱体包围盒为直径*直径*长度
            _physicalSizeX = diameter;
            _physicalSizeY = diameter;
            _physicalSizeZ = length;
            // 标准化锚点：吸笔原点在安装端（顶部）
            _physicalAnchor = PhysicalAnchor.TopCenter;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder AsRotaryTable(double radius)
        {
            _physicalType = PhysicalType.RotaryTable;
            _physicalParam1 = radius;
            _physicalSizeX = radius * 2;
            _physicalSizeY = radius * 2;
            _physicalSizeZ = 10; // 默认高度
            // 标准化锚点：旋转台原点在中心顶部
            _physicalAnchor = PhysicalAnchor.TopCenter;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder AsLinearGuide(double length)
        {
            _physicalType = PhysicalType.LinearGuide;
            _physicalParam1 = length;
            _physicalSizeX = length;
            _physicalSizeY = 20; // 默认宽度
            _physicalSizeZ = 10; // 默认高度
            // 标准化锚点：导轨原点在行程起点
            _physicalAnchor = PhysicalAnchor.StrokeStart;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder AsGripper()
        {
            _physicalType = PhysicalType.Gripper;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder AsMaterialSlot(double width, double height)
        {
            _physicalType = PhysicalType.MaterialSlot;
            _physicalSizeX = width;
            _physicalSizeY = height;
            _physicalSizeZ = 1; // 默认薄片
            _physicalAnchor = PhysicalAnchor.Center;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder WithAnchor(PhysicalAnchor anchor)
        {
            _physicalAnchor = anchor;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder Vertical()
        {
            _isVertical = true;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder Horizontal()
        {
            _isVertical = false;
            PropagateUpdate();
            return this;
        }

        public IMountPointBuilder Inverted()
        {
            _isInverted = true;
            PropagateUpdate();
            return this;
        }

        /// <summary>
        /// 向上冒泡通知根节点更新 Assembly 中的定义
        /// </summary>
        private void PropagateUpdate()
        {
            if (_parent != null)
            {
                _parent.PropagateUpdate();
            }
            else if (_asm != null)
            {
                // 根节点：更新或添加到 Assembly
                var def = ToDefinition();
                var idx = _asm.MountPoints.FindIndex(m => m.Name == _name);
                if (idx >= 0) _asm.MountPoints[idx] = def;
                else _asm.MountPoints.Add(def);
            }
        }

        /// <summary>
        /// 验证物理属性对齐规则。
        /// 如果发现冲突，抛出 BlueprintValidationException。
        /// </summary>
        private void ValidateAlignment()
        {
            // 如果标记为垂直，但行程向量没有 Z 分量（只有 X/Y），则为冲突
            if (_isVertical && Math.Abs(_sz) < 0.001 && (Math.Abs(_sx) > 0.001 || Math.Abs(_sy) > 0.001))
            {
                throw new BlueprintValidationException(
                    $"AlignmentConflict: 挂载点 '{_name}' 被标记为 Vertical()，但其 WithStroke 只有水平分量 ({_sx}, {_sy}, {_sz})。"
                );
            }
        }

        public MountPointDefinition ToDefinition()
        {
            // 执行物理对齐校验
            ValidateAlignment();

            // 递归地将所有子 Builder 转换为定义
            var childDefs = new List<MountPointDefinition>();
            foreach (var cb in _childBuilders)
            {
                childDefs.Add(cb.ToDefinition());
            }

            // 构建物理属性（如果有定义）
            PhysicalProperty? physical = null;
            if (_physicalType != PhysicalType.None)
            {
                physical = new PhysicalProperty(
                    _physicalType,
                    _physicalSizeX, _physicalSizeY, _physicalSizeZ,
                    _physicalAnchor,
                    _isVertical,
                    _isInverted,
                    _physicalParam1,
                    _physicalParam2
                );
            }
            
            return new MountPointDefinition(
                _name, 
                _parent?._name, 
                _linkedDevice, 
                _ox, _oy, _oz, 
                _rx, _ry, _rz,
                _sx, _sy, _sz,
                childDefs,
                physical
            );
        }
    }

    /// <summary>
    /// 蓝图校验异常。编译期发现物理配置冲突时抛出。
    /// </summary>
    public class BlueprintValidationException : Exception
    {
        public BlueprintValidationException(string message) : base(message) { }
    }

    internal sealed class LeadshineDriverBuilder : ILeadshineDriverBuilder
    {
        public LeadshineModel ModelType = LeadshineModel.DMC3000; public int CardIdValue = 0;
        public Dictionary<string, Action<AxisConfigBuilder>> AxisConfigs = new();
        public ILeadshineDriverBuilder Model(LeadshineModel m) { ModelType = m; return this; }
        public ILeadshineDriverBuilder CardId(int id) { CardIdValue = id; return this; }
        public ILeadshineDriverBuilder ConfigAxis(Enum axis, Action<AxisConfigBuilder> cfg) { AxisConfigs[axis.ToString()] = cfg; return this; }
    }

    internal sealed class ZMotionDriverBuilder : IZMotionDriverBuilder
    {
        public ZMotionModel ModelType = ZMotionModel.ZMC432; public string Ip = "127.0.0.1";
        public IZMotionDriverBuilder Model(ZMotionModel m) { ModelType = m; return this; }
        public IZMotionDriverBuilder IpAddress(string ip) { Ip = ip; return this; }
    }

    internal enum DriverType { Simulator, Leadshine, ZMotion }
    internal record BlueprintBoardDefinition(string Name, int CardId, BoardBuilder Builder);
    internal record BlueprintAxisDefinition(int Channel, string Name, double Min, double Max, double MaxVel, double MaxAcc, bool Reversed, bool IsVertical);
    internal record BlueprintCylinderDefinition(string Name, int DoOut, int? DoIn, int? FbOut, int? FbIn, int ActionTimeMs, bool IsVertical);
}
