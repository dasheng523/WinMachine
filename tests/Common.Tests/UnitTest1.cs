using Common.Fsm;
using Common.Lifecycle;
using FluentAssertions;

namespace Common.Tests;

public class StateMachineTests
{
    [Fact]
    public void Fire_KnownTransition_ShouldMoveState_AndRunEntryAction()
    {
        var entered = false;

        using var fsm = new StateMachineBuilder()
            .InitialState(MachineState.Initial)
            .In(MachineState.Initial, s => s.On(MachineTrigger.Connect, MachineState.Initializing))
            .WhenEntering(MachineState.Initializing, () => entered = true)
            .Build();

        fsm.Current.Should().Be(MachineState.Initial);

        fsm.Fire(MachineTrigger.Connect);

        fsm.Current.Should().Be(MachineState.Initializing);
        entered.Should().BeTrue();
    }

    [Fact]
    public void Fire_UnknownTransition_ShouldKeepState()
    {
        using var fsm = new StateMachineBuilder()
            .InitialState(MachineState.Initial)
            .Build();

        fsm.Fire(MachineTrigger.Start);

        fsm.Current.Should().Be(MachineState.Initial);
    }
}
