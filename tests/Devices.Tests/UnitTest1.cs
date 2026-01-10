using Devices.Motion.Implementations.Simulator;
using FluentAssertions;

namespace Devices.Tests;

public class SimulatorMotionControllerTests
{
    [Fact]
    public void MoveJog_ThenStop_ShouldReportDone()
    {
        using var motion = new SimulatorMotionController<int, int, int>();

        motion.Move_JOG(0, Devices.Motion.Abstractions.MotionDirection.Positive);
        motion.CheckDone(0).Should().BeFalse();

        motion.Stop(0);
        motion.CheckDone(0).Should().BeTrue();
    }

    [Fact]
    public void GoBackHome_ShouldSetAxisPositionToZero()
    {
        using var motion = new SimulatorMotionController<int, int, int>();

        motion.SetCommandPos(0, 123.0);
        motion.GetCommandPos(0).Should().Be(123.0);

        motion.GoBackHome(0);
        motion.GetCommandPos(0).Should().Be(0);
    }
}
