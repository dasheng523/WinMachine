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

        public IBoardBuilder AddCylinder(CylinderID cyl, int o, int i) { 
            var b = new CylinderBuilder(cyl.Name, o, i, this); 
            b.Commit(); 
            return this; 
        }
        public IBoardBuilder AddCylinder(CylinderID cyl, int o, int i, Action<ICylinderBuilder> cfg) { 
            var b = new CylinderBuilder(cyl.Name, o, i, this);
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
        private readonly string _name; private readonly int _o, _i; private readonly BoardBuilder _board;
        private int? _fo, _fi; private int _t = 200; private bool _vert = false;
        public CylinderBuilder(string n, int o, int i, BoardBuilder b) { _name = n; _o = o; _i = i; _board = b; }
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
        private double _ox, _oy, _oz;
        
        // 用于构建子节点
        private readonly List<MountPointDefinition> _children = new();

        public MountPointBuilder(string name, MountPointBuilder? parent, BlueprintAssembly? asm)
        {
            _name = name;
            _parent = parent;
            _asm = asm;
        }

        public IMountPointBuilder AttachedTo(object p) 
        { 
            // 语义上的 AttachedTo在链式 Mount 中通常隐含了，这里保留给顶层调用
            return this; 
        }

        public IMountPointBuilder LinkTo(object a) 
        { 
            _linkedDevice = a; 
            UpdateDefinition();
            return this; 
        }
        
        public IMountPointBuilder WithTransform(Func<double, double> t) => this; // 暂未存储 Transform

        public IMountPointBuilder WithOffset(double x, double y, double z) 
        { 
            _ox = x; _oy = y; _oz = z;
            UpdateDefinition();
            return this; 
        }

        public IMountPointBuilder Mount(string n) 
        { 
            var child = new MountPointBuilder(n, this, null);
            return child; 
        }
        
        public IMountPointBuilder Mount(string n, Action<IMountPointBuilder> c) 
        {
            var childBuilder = new MountPointBuilder(n, this, null);
            c(childBuilder);
            
            // 将构建好的子节点定义加入当前节点的子列表
            // 注意：这里需要递归地拿到 childBuilder 的定义
            var childDef = childBuilder.ToDefinition();
            _children.Add(childDef);
            
            UpdateDefinition();
            return this; 
        }

        private void UpdateDefinition()
        {
            // 如果是根节点，直接更新 Assembly 中的列表
            // 注意：这是一个简化实现。对于深层嵌套，我们通常只在 Build 结束时生成一次树。
            // 但在这里为了支持 fluent API 的随时更新，我们采用"最终提交"策略比较复杂。
            // 简单的做法是：MountPointBuilder 不直接操作 Assembly，而是只有根 Builder 操作。
            
            if (_parent == null && _asm != null)
            {
                // 根节点：更新或添加到 Assembly
                var def = ToDefinition();
                var idx = _asm.MountPoints.FindIndex(m => m.Name == _name);
                if (idx >= 0) _asm.MountPoints[idx] = def;
                else _asm.MountPoints.Add(def);
            }
        }

        public MountPointDefinition ToDefinition()
        {
            return new MountPointDefinition(
                _name, 
                _parent?._name, 
                _linkedDevice, 
                _ox, _oy, _oz, 
                new List<MountPointDefinition>(_children) // Copy
            );
        }
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
    internal record BlueprintCylinderDefinition(string Name, int DoOut, int DoIn, int? FbOut, int? FbIn, int ActionTimeMs, bool IsVertical);
}
