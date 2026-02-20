namespace MachineOrchestration.Core.Types;

/// <summary>电机动作</summary>
public abstract record MotorAction
{
    /// <summary>移动到位置</summary>
    public sealed record MoveTo(float Position, float Speed) : MotorAction;
    
    /// <summary>旋转到角度</summary>
    public sealed record RotateTo(float Angle, float Speed) : MotorAction;
    
    /// <summary>回零</summary>
    public sealed record Home : MotorAction
    {
        private Home() { }
        public static readonly Home Instance = new();
    }
    
    /// <summary>停止</summary>
    public sealed record Stop : MotorAction
    {
        private Stop() { }
        public static readonly Stop Instance = new();
    }
    
    private MotorAction() { }
}

/// <summary>执行器动作</summary>
public abstract record ActuatorAction
{
    /// <summary>气缸伸出</summary>
    public sealed record Extend : ActuatorAction
    {
        private Extend() { }
        public static readonly Extend Instance = new();
    }
    
    /// <summary>气缸缩回</summary>
    public sealed record Retract : ActuatorAction
    {
        private Retract() { }
        public static readonly Retract Instance = new();
    }
    
    /// <summary>夹爪闭合</summary>
    public sealed record Close : ActuatorAction
    {
        private Close() { }
        public static readonly Close Instance = new();
    }
    
    /// <summary>夹爪松开</summary>
    public sealed record Open : ActuatorAction
    {
        private Open() { }
        public static readonly Open Instance = new();
    }
    
    /// <summary>吸气装置吸气</summary>
    public sealed record Suction : ActuatorAction
    {
        private Suction() { }
        public static readonly Suction Instance = new();
    }
    
    /// <summary>吸气装置常规</summary>
    public sealed record Normal : ActuatorAction
    {
        private Normal() { }
        public static readonly Normal Instance = new();
    }
    
    /// <summary>指示灯开</summary>
    public sealed record On : ActuatorAction
    {
        private On() { }
        public static readonly On Instance = new();
    }
    
    /// <summary>指示灯关</summary>
    public sealed record Off : ActuatorAction
    {
        private Off() { }
        public static readonly Off Instance = new();
    }
    
    private ActuatorAction() { }
}

/// <summary>零件动作（和类型）</summary>
public abstract record PartAction
{
    public sealed record Motor(MotorAction Action) : PartAction;
    public sealed record Actuator(ActuatorAction Action) : PartAction;
    
    private PartAction() { }
}
