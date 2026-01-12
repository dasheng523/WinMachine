using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Core;
using Common.Hardware;
using Devices.Motion.Implementations.Simulator;
using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.Options;
using WinMachine.Configuration;
using WinMachine.Services;
using static LanguageExt.Prelude;

namespace WinMachine.Tests;

public class HardwareDslTests
{
    [Fact]
    public void CylinderCommand_WithExtendedSensor_ShouldFailWhenNotInPosition()
    {
        using var motion = new SimulatorMotionController<ushort, ushort, ushort>();
        using var motionSystem = new MotionSystem(new[] { new MotionBoard("Main", motion) });

        var map =
            from m0 in MachineMap.Empty
            from m1 in m0.DO("ScanSeat.CylValve").OnBoard("Main").Bit(0)
            from m2 in m1.DI("ScanSeat.CylExtended").OnBoard("Main").Bit(2)
            from m3 in m2.Cylinder1Do("ScanSeat.Cyl")
                .ValveDo("ScanSeat.CylValve")
                .OnMeans(CylinderCommand.Extend)
                .ExtendedDi("ScanSeat.CylExtended")
                .Commit()
            select m3;

        var opt = Options.Create(
            map.Match(
                Succ: m => m.ToSystemOptions(),
                Fail: e => throw new Exception(e.ToString())));

        var io = new IoResolver(motionSystem, opt);
        var cylinders = new CylinderResolver(io, opt);
        var axes = new AxisResolver(motionSystem, opt);
        var hw = new HardwareFacade(axes, motionSystem, io, cylinders);

        Fin<Unit> flow =
            from cyl in hw.Cylinders.Resolve("ScanSeat.Cyl")
            from _1 in cyl.Command(CylinderCommand.Extend)
            from ok in cyl.ExtendedSensor.Match(
                Some: s => s.ReadActive(),
                None: () => FinSucc(true))
            from _2 in ok
                ? FinSucc(unit)
                : FinFail<Unit>(LanguageExt.Common.Error.New("ScanSeat.Cyl 未到伸出位"))
            select unit;

        // Simulator 默认 DI=Off，因此应失败
        var err = flow.Match(
            Succ: _ => string.Empty,
            Fail: e => e.ToString());

        err.Should().NotBeEmpty();
        err.Should().Contain("未到伸出位");

        // 同时验证 DO 已被写为 On
        motion.GetOutput((ushort)0).Match(
            Succ: v => v.Should().Be(Level.On),
            Fail: e => throw new Exception(e.ToString()));
    }

    [Fact]
    public void CylinderCommand_WithExtendedSensor_ShouldSucceedWhenInputForcedHigh()
    {
        using var motion = new SimulatorMotionController<ushort, ushort, ushort>();
        using var motionSystem = new MotionSystem(new[] { new MotionBoard("Main", motion) });

        var map =
            from m0 in MachineMap.Empty
            from m1 in m0.DO("ScanSeat.CylValve").OnBoard("Main").Bit(0)
            from m2 in m1.DI("ScanSeat.CylExtended").OnBoard("Main").Bit(2)
            from m3 in m2.Cylinder1Do("ScanSeat.Cyl")
                .ValveDo("ScanSeat.CylValve")
                .OnMeans(CylinderCommand.Extend)
                .ExtendedDi("ScanSeat.CylExtended")
                .Commit()
            select m3;

        var opt = Options.Create(
            map.Match(
                Succ: m => m.ToSystemOptions(),
                Fail: e => throw new Exception(e.ToString())));

        // 用反射强行把模拟器输入置高，模拟“到位信号高有效”
        SetSimulatorInput(motion, bit: 2, Level.On);

        var io = new IoResolver(motionSystem, opt);
        var cylinders = new CylinderResolver(io, opt);
        var axes = new AxisResolver(motionSystem, opt);
        var hw = new HardwareFacade(axes, motionSystem, io, cylinders);

        Fin<Unit> flow =
            from cyl in hw.Cylinders.Resolve("ScanSeat.Cyl")
            from _1 in cyl.Command(CylinderCommand.Extend)
            from ok in cyl.ExtendedSensor.Match(
                Some: s => s.ReadActive(),
                None: () => FinSucc(true))
            from _2 in ok
                ? FinSucc(unit)
                : FinFail<Unit>(LanguageExt.Common.Error.New("ScanSeat.Cyl 未到伸出位"))
            select unit;

        flow.Match(
            Succ: _ => { },
            Fail: e => throw new Exception(e.ToString()));

        motion.GetOutput((ushort)0).Match(
            Succ: v => v.Should().Be(Level.On),
            Fail: e => throw new Exception(e.ToString()));
    }

    private static void SetSimulatorInput(SimulatorMotionController<ushort, ushort, ushort> motion, int bit, Level level)
    {
        var f = typeof(SimulatorMotionController<ushort, ushort, ushort>)
            .GetField("_inputs", BindingFlags.NonPublic | BindingFlags.Instance);

        f.Should().NotBeNull("SimulatorMotionController 应包含 _inputs 字段");

        var dict = (Dictionary<int, Level>)f!.GetValue(motion)!;
        dict[bit] = level;
    }
}
