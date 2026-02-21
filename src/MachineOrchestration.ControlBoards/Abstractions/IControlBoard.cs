using System;
using System.Threading.Tasks;
using LanguageExt;
using MachineOrchestration.ControlBoards.Types;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.ControlBoards.Abstractions;

/// <summary>
/// 控制板抽象接口（副作用边界）
/// 提供统一的控制板操作接口，支持多种控制板实现
/// </summary>
public interface IControlBoard
{
    /// <summary>
    /// 初始化控制板
    /// </summary>
    /// <returns>初始化结果，成功返回 Unit，失败返回错误</returns>
    Task<Either<ControlBoardError, Unit>> Initialize();
    
    /// <summary>
    /// 发送电机命令
    /// </summary>
    /// <param name="motorId">电机 ID</param>
    /// <param name="action">电机动作</param>
    /// <returns>执行结果，成功返回 Unit，失败返回错误</returns>
    Task<Either<ControlBoardError, Unit>> SendMotorCommand(
        MotorId motorId,
        MotorAction action);
    
    /// <summary>
    /// 发送执行器命令
    /// </summary>
    /// <param name="actuatorId">执行器 ID</param>
    /// <param name="action">执行器动作</param>
    /// <returns>执行结果，成功返回 Unit，失败返回错误</returns>
    Task<Either<ControlBoardError, Unit>> SendActuatorCommand(
        ActuatorId actuatorId,
        ActuatorAction action);
    
    /// <summary>
    /// 读取传感器数据
    /// </summary>
    /// <param name="sensorId">传感器 ID</param>
    /// <returns>传感器读数，成功返回读数，失败返回错误</returns>
    Task<Either<ControlBoardError, SensorReading>> ReadSensor(
        SensorId sensorId);
    
    /// <summary>
    /// 读取状态传感器（布尔值）
    /// </summary>
    /// <param name="stateSensorId">状态传感器 ID</param>
    /// <returns>传感器状态，成功返回布尔值，失败返回错误</returns>
    Task<Either<ControlBoardError, bool>> ReadStateSensor(
        StateSensorId stateSensorId);
    
    /// <summary>
    /// 紧急停止所有动作
    /// </summary>
    /// <returns>执行结果，成功返回 Unit，失败返回错误</returns>
    Task<Either<ControlBoardError, Unit>> EmergencyStop();
    
    /// <summary>
    /// 控制板状态流（响应式）
    /// 使用 System.Reactive 暴露状态变化
    /// </summary>
    IObservable<ControlBoardState> StateStream { get; }
}
