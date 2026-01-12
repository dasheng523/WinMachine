using System;
using System.Collections.Generic;
using Common.Hardware;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace WinMachine.Configuration;

/// <summary>
/// 以 Fin + Linq（do-notation）方式构建映射：每一步返回 Fin&lt;MachineMap&gt;，失败可短路。
/// 当前覆盖：Axis/DI/DO/单DO气缸。
/// </summary>
public sealed class MachineMap
{
    private readonly SystemOptions _opt;

    private MachineMap(SystemOptions opt) => _opt = opt;

    public static Fin<MachineMap> Empty => FinSucc(new MachineMap(new SystemOptions()));

    public SystemOptions ToSystemOptions()
    {
        // 返回一份隔离的拷贝，避免外部后续修改影响已构建 map
        var axisMap = new Dictionary<string, AxisRefOptions>(_opt.AxisMap ?? [], StringComparer.OrdinalIgnoreCase);

        var di = new Dictionary<string, IoRefOptions>(_opt.IoMap?.Di ?? [], StringComparer.OrdinalIgnoreCase);
        var @do = new Dictionary<string, IoRefOptions>(_opt.IoMap?.Do ?? [], StringComparer.OrdinalIgnoreCase);

        var cyl = new Dictionary<string, SingleSolenoidCylinderOptions>(_opt.CylinderMap ?? [], StringComparer.OrdinalIgnoreCase);

        var sensors = new Dictionary<string, SensorOptions>(_opt.SensorMap ?? [], StringComparer.OrdinalIgnoreCase);

        return new SystemOptions
        {
            UseSimulator = _opt.UseSimulator,
            SuggestedAxisKeys = _opt.SuggestedAxisKeys,
            MotionBoards = _opt.MotionBoards,
            AxisMap = axisMap,
            IoMap = new IoMapOptions { Di = di, Do = @do },
            CylinderMap = cyl,
            SensorMap = sensors
        };
    }

    public AxisBuilder Axis(string name) => new(this, name);

    public IoBuilder DI(string name) => new(this, isDi: true, name);

    public IoBuilder DO(string name) => new(this, isDi: false, name);

    public SingleSolenoidCylinderBuilder Cylinder1Do(string name) => new(this, name);

    public BoolSensorBuilder BoolSensor(string name) => new(this, name);

    private static Fin<string> RequireName(string kind, string name) =>
        string.IsNullOrWhiteSpace(name)
            ? FinFail<string>(Error.New($"{kind} 名称不能为空"))
            : FinSucc(name);

    private MachineMap WithAxis(string name, AxisRefOptions v)
    {
        var next = ToSystemOptions();
        next.AxisMap[name] = v;
        return new MachineMap(next);
    }

    private MachineMap WithIo(bool isDi, string name, IoRefOptions v)
    {
        var next = ToSystemOptions();
        if (isDi) next.IoMap.Di[name] = v;
        else next.IoMap.Do[name] = v;
        return new MachineMap(next);
    }

    private MachineMap WithCylinder(string name, SingleSolenoidCylinderOptions v)
    {
        var next = ToSystemOptions();
        next.CylinderMap[name] = v;
        return new MachineMap(next);
    }

    private MachineMap WithSensor(string name, SensorOptions v)
    {
        var next = ToSystemOptions();
        next.SensorMap[name] = v;
        return new MachineMap(next);
    }

    public sealed class AxisBuilder
    {
        private readonly MachineMap _map;
        private readonly string _name;
        private string? _board;

        internal AxisBuilder(MachineMap map, string name)
        {
            _map = map;
            _name = name;
        }

        public AxisBuilder OnBoard(string? board)
        {
            _board = board;
            return this;
        }

        public Fin<MachineMap> AxisNo(ushort axisNo) =>
            from n in RequireName("Axis", _name)
            select _map.WithAxis(n, new AxisRefOptions { Board = _board, Axis = axisNo });
    }

    public sealed class IoBuilder
    {
        private readonly MachineMap _map;
        private readonly bool _isDi;
        private readonly string _name;
        private string? _board;

        internal IoBuilder(MachineMap map, bool isDi, string name)
        {
            _map = map;
            _isDi = isDi;
            _name = name;
        }

        public IoBuilder OnBoard(string? board)
        {
            _board = board;
            return this;
        }

        public Fin<MachineMap> Bit(ushort bit) =>
            from n in RequireName(_isDi ? "DI" : "DO", _name)
            select _map.WithIo(_isDi, n, new IoRefOptions { Board = _board, Bit = bit });
    }

    public sealed class SingleSolenoidCylinderBuilder
    {
        private readonly MachineMap _map;
        private readonly string _name;

        private string? _valveDo;
        private CylinderCommand _onMeans = CylinderCommand.Extend;
        private string? _extendedDi;
        private string? _retractedDi;
        private string? _healthOkDi;

        internal SingleSolenoidCylinderBuilder(MachineMap map, string name)
        {
            _map = map;
            _name = name;
        }

        public SingleSolenoidCylinderBuilder ValveDo(string valveDo)
        {
            _valveDo = valveDo;
            return this;
        }

        public SingleSolenoidCylinderBuilder OnMeans(CylinderCommand cmd)
        {
            _onMeans = cmd;
            return this;
        }

        public SingleSolenoidCylinderBuilder ExtendedDi(string? di)
        {
            _extendedDi = di;
            return this;
        }

        public SingleSolenoidCylinderBuilder RetractedDi(string? di)
        {
            _retractedDi = di;
            return this;
        }

        public SingleSolenoidCylinderBuilder HealthOkDi(string? di)
        {
            _healthOkDi = di;
            return this;
        }

        public Fin<MachineMap> Commit() =>
            from n in RequireName("Cylinder", _name)
            from vd in RequireName("ValveDo", _valveDo ?? string.Empty)
            select _map.WithCylinder(n, new SingleSolenoidCylinderOptions
            {
                ValveDo = vd,
                OnMeans = _onMeans,
                ExtendedDi = _extendedDi,
                RetractedDi = _retractedDi,
                HealthOkDi = _healthOkDi
            });
    }

    public sealed class BoolSensorBuilder
    {
        private readonly MachineMap _map;
        private readonly string _name;

        private SensorOptions _sensor = new() { Kind = SensorKind.DiLevel };

        internal BoolSensorBuilder(MachineMap map, string name)
        {
            _map = map;
            _name = name;
        }

        public Fin<MachineMap> FromDi(string diLogicalName)
        {
            _sensor = new SensorOptions
            {
                Kind = SensorKind.DiLevel,
                Di = diLogicalName
            };

            return Commit();
        }

        public ModbusBoolSensorBuilder FromModbus(string portName, int baudRate) =>
            new(_map, _name, portName, baudRate);

        private Fin<MachineMap> Commit() =>
            from n in RequireName("Sensor", _name)
            select _map.WithSensor(n, _sensor);
    }

    public sealed class ModbusBoolSensorBuilder
    {
        private readonly MachineMap _map;
        private readonly string _name;
        private readonly string _portName;
        private readonly int _baudRate;
        private byte _slaveId;

        internal ModbusBoolSensorBuilder(MachineMap map, string name, string portName, int baudRate)
        {
            _map = map;
            _name = name;
            _portName = portName;
            _baudRate = baudRate;
        }

        public ModbusBoolSensorBuilder Slave(byte slaveId)
        {
            _slaveId = slaveId;
            return this;
        }

        /// <summary>
        /// 线圈地址（Coil），高有效。
        /// 返回 Fin&lt;MachineMap&gt; 以继续 Linq 链。
        /// </summary>
        public Fin<MachineMap> Coil(ushort address) =>
            from n in RequireName("Sensor", _name)
            from p in RequireName("PortName", _portName)
            select _map.WithSensor(n, new SensorOptions
            {
                Kind = SensorKind.ModbusCoil,
                Modbus = new ModbusSensorOptions
                {
                    PortName = p,
                    BaudRate = _baudRate,
                    SlaveId = _slaveId,
                    Address = address,
                    Count = 1
                }
            });
    }
}
