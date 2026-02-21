using System;
using System.Threading.Tasks;

namespace MachineOrchestration.ControlBoards.Sdk;

/// <summary>
/// 正运动 SDK 接口（模拟）
/// 实际项目中应替换为真实的正运动 SDK
/// </summary>
public interface IZhengYunDongSdk
{
    /// <summary>连接到控制板</summary>
    Task<bool> ConnectAsync(string ipAddress, int port);
    
    /// <summary>断开连接</summary>
    Task DisconnectAsync();
    
    /// <summary>检查连接状态</summary>
    bool IsConnected { get; }
    
    /// <summary>发送电机移动命令</summary>
    Task<bool> MoveMotorAsync(int axisId, float position, float speed);
    
    /// <summary>发送电机旋转命令</summary>
    Task<bool> RotateMotorAsync(int axisId, float angle, float speed);
    
    /// <summary>发送电机回零命令</summary>
    Task<bool> HomeMotorAsync(int axisId);
    
    /// <summary>停止电机</summary>
    Task<bool> StopMotorAsync(int axisId);
    
    /// <summary>设置输出端口状态</summary>
    Task<bool> SetOutputAsync(int portId, bool state);
    
    /// <summary>读取输入端口状态</summary>
    Task<bool> ReadInputAsync(int portId);
    
    /// <summary>读取模拟输入</summary>
    Task<float> ReadAnalogInputAsync(int channelId);
    
    /// <summary>获取电机当前位置</summary>
    Task<float> GetMotorPositionAsync(int axisId);
    
    /// <summary>获取电机当前速度</summary>
    Task<float> GetMotorSpeedAsync(int axisId);
    
    /// <summary>检查电机是否在运动</summary>
    Task<bool> IsMotorMovingAsync(int axisId);
    
    /// <summary>紧急停止所有轴</summary>
    Task<bool> EmergencyStopAllAsync();
}
