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
        public BlueprintAssembly(string name) => Name = name;
    }

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

        public IMountPointBuilder Mount(string name) => new MountPointBuilder();
        public IMachineBlueprintBuilder Mount(string name, Action<IMountPointBuilder> config) { config(Mount(name)); return this; }

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

        public IAxisBuilder AddAxis(AxisID axis, int channel) { var b = new AxisBuilder(channel, axis.Name, this); b.Commit(); return b; }
        public IBoardBuilder AddAxis(AxisID axis, int channel, Action<IAxisBuilder> cfg) { cfg(AddAxis(axis, channel)); return this; }
        public ICylinderBuilder AddCylinder(CylinderID cyl, int o, int i) { var b = new CylinderBuilder(cyl.Name, o, i, this); b.Commit(); return b; }
        public IBoardBuilder AddCylinder(CylinderID cyl, int o, int i, Action<ICylinderBuilder> cfg) { cfg(AddCylinder(cyl, o, i)); return this; }
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
        public IMountPointBuilder AttachedTo(object p) => this;
        public IMountPointBuilder LinkTo(object a) => this;
        public IMountPointBuilder WithTransform(Func<double, double> t) => this;
        public IMountPointBuilder WithOffset(double x, double y, double z) => this;
        public IMountPointBuilder Mount(string n) => new MountPointBuilder();
        public IMountPointBuilder Mount(string n, Action<IMountPointBuilder> c) { c(Mount(n)); return this; }
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
