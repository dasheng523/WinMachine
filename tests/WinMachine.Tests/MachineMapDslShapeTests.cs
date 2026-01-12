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
}
