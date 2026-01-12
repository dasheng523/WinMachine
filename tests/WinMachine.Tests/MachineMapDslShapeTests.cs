using System;
using FluentAssertions;
using WinMachine.Configuration;
using static LanguageExt.Prelude;

namespace WinMachine.Tests;

public class MachineMapDslShapeTests
{
    [Fact]
    public void MachineMapDsl_ShouldSupportModbusBoolSensorShape()
    {
        var map =
            from m0 in MachineMap.Empty
            from m1 in m0.BoolSensor("ScanSeat.Extended")
                .FromModbus("COM3", 115200)
                .Slave(1)
                .Coil(10)
            select m1;

        var opt = map.Match(
            Succ: m => m.ToSystemOptions(),
            Fail: e => throw new Exception(e.ToString()));

        opt.SensorMap.Should().ContainKey("ScanSeat.Extended");
        var s = opt.SensorMap["ScanSeat.Extended"];
        s.Kind.Should().Be(SensorKind.ModbusCoil);
        s.Modbus!.PortName.Should().Be("COM3");
        s.Modbus!.BaudRate.Should().Be(115200);
        s.Modbus!.SlaveId.Should().Be(1);
        s.Modbus!.Address.Should().Be(10);
    }

    [Fact]
    public void MachineMapDsl_ShouldSupportModbusDoubleHoldingRegisterShape()
    {
        var map =
            from m0 in MachineMap.Empty
            from m1 in m0.DoubleSensor("Pressure.Gauge1")
                .FromModbus("COM3", 115200)
                .Slave(1)
                .Count(2)
                .Scale(0.1)
                .HoldingRegister(100)
            select m1;

        var opt = map.Match(
            Succ: m => m.ToSystemOptions(),
            Fail: e => throw new Exception(e.ToString()));

        opt.SensorMap.Should().ContainKey("Pressure.Gauge1");
        var s = opt.SensorMap["Pressure.Gauge1"];
        s.Kind.Should().Be(SensorKind.ModbusHoldingRegister);
        s.Modbus!.PortName.Should().Be("COM3");
        s.Modbus!.BaudRate.Should().Be(115200);
        s.Modbus!.SlaveId.Should().Be(1);
        s.Modbus!.Address.Should().Be(100);
        s.Modbus!.Count.Should().Be(2);
        s.Modbus!.Scale.Should().Be(0.1);
    }

    [Fact]
    public void MachineMapDsl_ShouldSupportSerialStringSensorShape()
    {
        var map =
            from m0 in MachineMap.Empty
            from m1 in m0.StringSensor("Scanner.Main")
                .FromSerialLine("COM5", 9600)
                .Address("A")
                .Commit()
            select m1;

        var opt = map.Match(
            Succ: m => m.ToSystemOptions(),
            Fail: e => throw new Exception(e.ToString()));

        opt.SensorMap.Should().ContainKey("Scanner.Main");
        var s = opt.SensorMap["Scanner.Main"];
        s.Kind.Should().Be(SensorKind.SerialLine);
        s.Serial!.PortName.Should().Be("COM5");
        s.Serial!.BaudRate.Should().Be(9600);
        s.Serial!.Address.Should().Be("A");
    }

    [Fact]
    public void MachineMapDsl_ShouldSupportCylinder2DoShape()
    {
        var map =
            from m0 in MachineMap.Empty
            from m1 in m0.DO("ScanSeat.Extend").OnBoard("Main").Bit(0)
            from m2 in m1.DO("ScanSeat.Retract").OnBoard("Main").Bit(1)
            from m3 in m2.BoolSensor("ScanSeat.Extended")
                .FromModbus("COM3", 115200)
                .Slave(1)
                .Coil(10)
            from m4 in m3.BoolSensor("ScanSeat.Retracted")
                .FromModbus("COM3", 115200)
                .Slave(1)
                .Coil(11)
            from m5 in m4.Cylinder2Do("ScanSeatCyl")
                .ExtendDo("ScanSeat.Extend")
                .RetractDo("ScanSeat.Retract")
                .Extended("ScanSeat.Extended")
                .Retracted("ScanSeat.Retracted")
                .Commit()
            select m5;

        var opt = map.Match(
            Succ: m => m.ToSystemOptions(),
            Fail: e => throw new Exception(e.ToString()));

        opt.Cylinder2Map.Should().ContainKey("ScanSeatCyl");
        var c = opt.Cylinder2Map["ScanSeatCyl"];
        c.ExtendDo.Should().Be("ScanSeat.Extend");
        c.RetractDo.Should().Be("ScanSeat.Retract");
        c.ExtendedSensor.Should().Be("ScanSeat.Extended");
        c.RetractedSensor.Should().Be("ScanSeat.Retracted");
    }
}
