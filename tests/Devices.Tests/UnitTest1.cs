using System;
using Devices.Motion.Implementations.Simulator;
using FluentAssertions;
using LanguageExt;
using LUnit = LanguageExt.Unit;

namespace Devices.Tests;

public class SimulatorMotionControllerTests
{
    private static void Expect(Fin<LUnit> fin) =>
        fin.Match(
            Succ: _ => { },
            Fail: err => throw new Exception(err.ToString()));

    private static T Expect<T>(Fin<T> fin) =>
        fin.Match(
            Succ: v => v,
            Fail: err => throw new Exception(err.ToString()));

    [Fact]
    public void MoveJog_ThenStop_ShouldReportDone()
    {
        using var motion = new SimulatorMotionController<int, int, int>();

        Expect(motion.Move_JOG(0, Devices.Motion.Abstractions.MotionDirection.Positive));
        Expect(motion.CheckDone(0)).Should().BeFalse();

        Expect(motion.Stop(0));
        Expect(motion.CheckDone(0)).Should().BeTrue();
    }

    [Fact]
    public void GoBackHome_ShouldSetAxisPositionToZero()
    {
        using var motion = new SimulatorMotionController<int, int, int>();

        Expect(motion.SetCommandPos(0, 123.0));
        Expect(motion.GetCommandPos(0)).Should().Be(123.0);

        Expect(motion.GoBackHome(0));
        Expect(motion.GetCommandPos(0)).Should().Be(0);
    }
}
