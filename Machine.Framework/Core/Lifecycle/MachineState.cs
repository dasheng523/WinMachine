namespace Machine.Framework.Core.Lifecycle
{
    public enum MachineState
    {
        Initial,
        Initializing,
        WaitHome,
        Homing,
        Idle,
        Running,
        Paused,
        Stopping,
        Faulted
    }

    public enum MachineTrigger
    {
        Connect,
        Connected,
        Home,
        Homed,
        Start,
        Pause,
        Resume,
        Stop,
        Stopped,
        ErrorOccurred,
        Reset
    }
}


