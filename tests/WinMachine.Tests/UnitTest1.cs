using System;
using System.Collections.Generic;
using Common.Lifecycle;
using Devices.Motion.Implementations.Simulator;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using WinMachine.Services;

namespace WinMachine.Tests;

public class MachineManagerIntegrationStyleTests
{
    [Fact]
    public void Connect_Home_Start_Stop_ShouldReachExpectedStates_WithVirtualTime()
    {
        var scheduler = new TestScheduler();
        using var motion = new SimulatorMotionController<int, int, int>();
        using var manager = new MachineManager(motion, scheduler);

        var states = new List<MachineState>();
        using var sub = manager.State.Subscribe(states.Add);

        manager.CurrentState.Should().Be(MachineState.Initial);

        manager.Connect();
        manager.CurrentState.Should().Be(MachineState.Initializing);

        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
        manager.CurrentState.Should().Be(MachineState.WaitHome);

        manager.Home();
        manager.CurrentState.Should().Be(MachineState.Homing);

        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks);
        manager.CurrentState.Should().Be(MachineState.Idle);

        manager.Start();
        manager.CurrentState.Should().Be(MachineState.Running);

        manager.Stop();
        manager.CurrentState.Should().Be(MachineState.Idle);

        states.Should().ContainInOrder(
            MachineState.Initial,
            MachineState.Initializing,
            MachineState.WaitHome,
            MachineState.Homing,
            MachineState.Idle,
            MachineState.Running,
            MachineState.Stopping,
            MachineState.Idle);
    }
}
