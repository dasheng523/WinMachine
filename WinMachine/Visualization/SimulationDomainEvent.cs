namespace WinMachine;

internal abstract record SimulationDomainEvent;

internal enum TransferGripAction
{
    Grab,
    ReleaseSwap
}

internal sealed record TransferGripEvent(TransferSide Side, TransferGripAction Action) : SimulationDomainEvent;
