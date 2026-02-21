using System;
using LanguageExt;
using MachineOrchestration.Core.Types;
using MachineOrchestration.Dsl.Ast;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Dsl.Interpreter;

/// <summary>程序计数器</summary>
/// <remarks>
/// 跟踪当前执行位置。使用路径表示嵌套语句中的位置。
/// 验证：需求 15.2
/// </remarks>
public readonly record struct ProgramCounter(Seq<int> Path)
{
    /// <summary>初始程序计数器（指向第一条语句）</summary>
    public static readonly ProgramCounter Initial = new(Seq<int>());
    
    /// <summary>前进到下一条语句</summary>
    public ProgramCounter Next()
    {
        if (Path.IsEmpty)
            return new ProgramCounter(Seq1(1)); // 从第一条语句前进到第二条
        
        return new ProgramCounter(Path.Init.Add(Path.Last + 1));
    }
    
    /// <summary>进入嵌套语句</summary>
    public ProgramCounter Enter(int index) => new(Path.Add(index));
    
    /// <summary>退出嵌套语句</summary>
    public ProgramCounter Exit() => 
        Path.IsEmpty ? this : new ProgramCounter(Path.Init);
}

/// <summary>零件状态（和类型）</summary>
/// <remarks>
/// 表示零件的运行时状态。
/// 验证：需求 15.2
/// </remarks>
public abstract record PartState
{
    /// <summary>电机状态</summary>
    public sealed record Motor(
        float CurrentPosition,
        float CurrentSpeed,
        bool IsHomed,
        bool IsMoving) : PartState;
    
    /// <summary>执行器状态</summary>
    public sealed record Actuator(
        ActuatorStateValue State,
        bool IsTransitioning) : PartState;
    
    /// <summary>传感器状态</summary>
    public sealed record Sensor(
        Option<SensorReading> LastReading,
        DateTime LastReadTime) : PartState;
    
    private PartState() { }
}

/// <summary>执行器状态值</summary>
public enum ActuatorStateValue
{
    /// <summary>气缸伸出</summary>
    Extended,
    
    /// <summary>气缸缩回</summary>
    Retracted,
    
    /// <summary>夹爪闭合</summary>
    Closed,
    
    /// <summary>夹爪松开</summary>
    Opened,
    
    /// <summary>吸气装置吸气</summary>
    Suctioning,
    
    /// <summary>吸气装置常规</summary>
    Normal,
    
    /// <summary>指示灯开</summary>
    On,
    
    /// <summary>指示灯关</summary>
    Off,
    
    /// <summary>未知状态</summary>
    Unknown
}

/// <summary>传感器读数（和类型）</summary>
/// <remarks>
/// 表示不同类型传感器的读数。
/// 验证：需求 15.2
/// </remarks>
public abstract record SensorReading
{
    /// <summary>布尔传感器读数（如状态传感器）</summary>
    public sealed record Boolean(bool Value) : SensorReading;
    
    /// <summary>数值传感器读数（如压力、千分表）</summary>
    public sealed record Numeric(float Value) : SensorReading;
    
    /// <summary>字符串传感器读数（如扫码器）</summary>
    public sealed record Text(string Value) : SensorReading;
    
    private SensorReading() { }
}

/// <summary>机器状态</summary>
/// <remarks>
/// 存储所有零件的当前状态。使用不可变映射。
/// 验证：需求 15.2
/// </remarks>
public sealed record MachineState(
    HashMap<EntityId, PartState> PartStates)
{
    /// <summary>空机器状态</summary>
    public static readonly MachineState Empty = new(HashMap<EntityId, PartState>());
    
    /// <summary>更新零件状态</summary>
    public MachineState UpdatePartState(EntityId entityId, PartState state) =>
        this with { PartStates = PartStates.AddOrUpdate(entityId, state) };
    
    /// <summary>获取零件状态</summary>
    public Option<PartState> GetPartState(EntityId entityId) =>
        PartStates.Find(entityId);
}

/// <summary>值类型（用于变量绑定）</summary>
/// <remarks>
/// 表示 DSL 中可以存储的值类型。
/// 验证：需求 15.2
/// </remarks>
public abstract record Value
{
    public sealed record Integer(int Val) : Value;
    public sealed record Float(float Val) : Value;
    public sealed record Boolean(bool Val) : Value;
    public sealed record String(string Val) : Value;
    
    private Value() { }
}

/// <summary>栈帧</summary>
/// <remarks>
/// 表示调用栈中的一帧，用于跟踪嵌套执行上下文。
/// 验证：需求 15.2
/// </remarks>
public sealed record StackFrame(
    Statement Statement,
    ProgramCounter ReturnAddress,
    HashMap<string, Value> LocalBindings)
{
    /// <summary>创建新栈帧</summary>
    public static StackFrame Create(Statement statement, ProgramCounter returnAddress) =>
        new(statement, returnAddress, HashMap<string, Value>());
}

/// <summary>执行状态</summary>
/// <remarks>
/// 表示 DSL 程序的完整执行状态。所有字段不可变。
/// 验证：需求 15.2
/// </remarks>
public sealed record ExecutionState(
    ProgramCounter Counter,
    MachineState MachineState,
    Seq<StackFrame> CallStack,
    HashMap<string, Value> Bindings,
    Option<DateTime> WaitUntil,
    bool IsComplete,
    Option<string> ErrorMessage)
{
    /// <summary>创建初始执行状态</summary>
    public static ExecutionState Initial(MachineState initialMachineState) =>
        new(
            ProgramCounter.Initial,
            initialMachineState,
            Seq<StackFrame>(),
            HashMap<string, Value>(),
            None,
            false,
            None);
    
    /// <summary>标记为完成</summary>
    public ExecutionState MarkComplete() =>
        this with { IsComplete = true };
    
    /// <summary>标记为错误</summary>
    public ExecutionState MarkError(string error) =>
        this with { IsComplete = true, ErrorMessage = Some(error) };
    
    /// <summary>更新程序计数器</summary>
    public ExecutionState UpdateCounter(ProgramCounter counter) =>
        this with { Counter = counter };
    
    /// <summary>更新机器状态</summary>
    public ExecutionState UpdateMachineState(MachineState machineState) =>
        this with { MachineState = machineState };
    
    /// <summary>设置等待时间</summary>
    public ExecutionState SetWaitUntil(DateTime waitUntil) =>
        this with { WaitUntil = Some(waitUntil) };
    
    /// <summary>清除等待时间</summary>
    public ExecutionState ClearWait() =>
        this with { WaitUntil = None };
    
    /// <summary>压入栈帧</summary>
    public ExecutionState PushFrame(StackFrame frame) =>
        this with { CallStack = CallStack.Add(frame) };
    
    /// <summary>弹出栈帧</summary>
    public ExecutionState PopFrame() =>
        CallStack.IsEmpty 
            ? this 
            : this with { CallStack = CallStack.Init };
    
    /// <summary>绑定变量</summary>
    public ExecutionState Bind(string name, Value value) =>
        this with { Bindings = Bindings.AddOrUpdate(name, value) };
    
    /// <summary>获取变量</summary>
    public Option<Value> GetBinding(string name) =>
        Bindings.Find(name);
}

