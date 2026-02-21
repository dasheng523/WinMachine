using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MachineOrchestration.ControlBoards.Sdk;

/// <summary>
/// 模拟正运动 SDK 实现（用于测试）
/// </summary>
public sealed class MockZhengYunDongSdk : IZhengYunDongSdk
{
    private readonly ConcurrentDictionary<int, float> _motorPositions = new();
    private readonly ConcurrentDictionary<int, float> _motorSpeeds = new();
    private readonly ConcurrentDictionary<int, bool> _motorMoving = new();
    private readonly ConcurrentDictionary<int, bool> _outputStates = new();
    private readonly ConcurrentDictionary<int, bool> _inputStates = new();
    private readonly Random _random = new();
    private bool _isConnected;
    private readonly bool _simulateErrors;
    private readonly double _errorProbability;

    public MockZhengYunDongSdk(bool simulateErrors = false, double errorProbability = 0.1)
    {
        _simulateErrors = simulateErrors;
        _errorProbability = errorProbability;
    }

    public bool IsConnected => _isConnected;

    public async Task<bool> ConnectAsync(string ipAddress, int port)
    {
        await Task.Delay(50); // 模拟连接延迟
        
        if (_simulateErrors && _random.NextDouble() < _errorProbability)
        {
            return false; // 模拟连接失败
        }
        
        _isConnected = true;
        return true;
    }

    public async Task DisconnectAsync()
    {
        await Task.Delay(10);
        _isConnected = false;
    }

    public async Task<bool> MoveMotorAsync(int axisId, float position, float speed)
    {
        if (!_isConnected) return false;
        
        await Task.Delay(20);
        
        if (_simulateErrors && _random.NextDouble() < _errorProbability)
        {
            throw new InvalidOperationException($"Motor {axisId} command failed");
        }
        
        _motorPositions[axisId] = position;
        _motorSpeeds[axisId] = speed;
        _motorMoving[axisId] = true;
        
        // 模拟运动完成
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            _motorMoving[axisId] = false;
            _motorSpeeds[axisId] = 0f;
        });
        
        return true;
    }

    public async Task<bool> RotateMotorAsync(int axisId, float angle, float speed)
    {
        if (!_isConnected) return false;
        
        await Task.Delay(20);
        
        if (_simulateErrors && _random.NextDouble() < _errorProbability)
        {
            throw new InvalidOperationException($"Motor {axisId} rotation failed");
        }
        
        _motorPositions[axisId] = angle;
        _motorSpeeds[axisId] = speed;
        _motorMoving[axisId] = true;
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            _motorMoving[axisId] = false;
            _motorSpeeds[axisId] = 0f;
        });
        
        return true;
    }

    public async Task<bool> HomeMotorAsync(int axisId)
    {
        if (!_isConnected) return false;
        
        await Task.Delay(20);
        
        if (_simulateErrors && _random.NextDouble() < _errorProbability)
        {
            throw new InvalidOperationException($"Motor {axisId} homing failed");
        }
        
        _motorPositions[axisId] = 0f;
        _motorMoving[axisId] = true;
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(150);
            _motorMoving[axisId] = false;
            _motorSpeeds[axisId] = 0f;
        });
        
        return true;
    }

    public async Task<bool> StopMotorAsync(int axisId)
    {
        if (!_isConnected) return false;
        
        await Task.Delay(10);
        _motorMoving[axisId] = false;
        _motorSpeeds[axisId] = 0f;
        return true;
    }

    public async Task<bool> SetOutputAsync(int portId, bool state)
    {
        if (!_isConnected) return false;
        
        await Task.Delay(10);
        
        if (_simulateErrors && _random.NextDouble() < _errorProbability)
        {
            throw new InvalidOperationException($"Output {portId} set failed");
        }
        
        _outputStates[portId] = state;
        return true;
    }

    public async Task<bool> ReadInputAsync(int portId)
    {
        if (!_isConnected) 
            throw new InvalidOperationException("Not connected");
        
        await Task.Delay(5);
        
        if (_simulateErrors && _random.NextDouble() < _errorProbability)
        {
            throw new InvalidOperationException($"Input {portId} read failed");
        }
        
        return _inputStates.GetOrAdd(portId, _ => _random.Next(2) == 1);
    }

    public async Task<float> ReadAnalogInputAsync(int channelId)
    {
        if (!_isConnected) 
            throw new InvalidOperationException("Not connected");
        
        await Task.Delay(5);
        
        if (_simulateErrors && _random.NextDouble() < _errorProbability)
        {
            throw new InvalidOperationException($"Analog input {channelId} read failed");
        }
        
        return (float)(_random.NextDouble() * 1000);
    }

    public async Task<float> GetMotorPositionAsync(int axisId)
    {
        if (!_isConnected) 
            throw new InvalidOperationException("Not connected");
        
        await Task.Delay(5);
        return _motorPositions.GetOrAdd(axisId, 0f);
    }

    public async Task<float> GetMotorSpeedAsync(int axisId)
    {
        if (!_isConnected) 
            throw new InvalidOperationException("Not connected");
        
        await Task.Delay(5);
        return _motorSpeeds.GetOrAdd(axisId, 0f);
    }

    public async Task<bool> IsMotorMovingAsync(int axisId)
    {
        if (!_isConnected) 
            throw new InvalidOperationException("Not connected");
        
        await Task.Delay(5);
        return _motorMoving.GetOrAdd(axisId, false);
    }

    public async Task<bool> EmergencyStopAllAsync()
    {
        if (!_isConnected) return false;
        
        await Task.Delay(10);
        
        foreach (var axisId in _motorMoving.Keys)
        {
            _motorMoving[axisId] = false;
            _motorSpeeds[axisId] = 0f;
        }
        
        return true;
    }
    
    // 测试辅助方法
    public void SetInputState(int portId, bool state)
    {
        _inputStates[portId] = state;
    }
}
