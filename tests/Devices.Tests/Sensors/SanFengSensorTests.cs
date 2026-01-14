using System;
using Devices.Sensors.Serial;
using Devices.Sensors.Runners;
using Common.Hardware;
using FluentAssertions;
using LanguageExt;
using Moq;
using Xunit;
using static LanguageExt.Prelude;
using Devices.Sensors.Core;

namespace Devices.Tests.Sensors;

/// <summary>
/// 侧重于测试传感器的业务逻辑（协议流程），通过 Runner 执行。
/// </summary>
public class SanFengSensorTests
{
    private readonly Mock<ISerialPortPool> _poolMock;
    private readonly Mock<ITextLinePort> _portMock;
    private readonly SerialRunner _runner;
    private readonly SanFengMicrometerOptions _options;

    public SanFengSensorTests()
    {
        _poolMock = new Mock<ISerialPortPool>();
        _portMock = new Mock<ITextLinePort>();
        
        _options = new SanFengMicrometerOptions 
        { 
            PortName = "COM3", 
            TriggerCommand = "1",
            MaxAttempts = 3 
        };

        // Setup Pool to return our mocked Port
        _poolMock.Setup(p => p.GetOrCreateTextLinePort(It.IsAny<SerialLineCommandOptions>()))
                 .Returns(_portMock.Object);
        _poolMock.Setup(p => p.GetLock(It.IsAny<SerialLineCommandOptions>()))
                 .Returns(new object());
                 
        _runner = new SerialRunner(_poolMock.Object);
    }

    [Fact]
    public void Read_ShouldParseNormalValue()
    {
        var sensor = new SanFengMicrometerSensor("SF01", _runner, _options);

        // Scenario: Success on first try
        _portMock.Setup(p => p.ReadLine()).Returns("01A+0012.345");
        
        var result = sensor.Read();
        
        result.IsSucc.Should().BeTrue();
        result.IfSucc(v => v.Should().Be(12.345));
        
        // Protocol check: Discard -> Write(Trig) -> Read
        _portMock.Verify(p => p.DiscardInBuffer(), Times.Once);
        _portMock.Verify(p => p.Write("1"), Times.Once);
    }

    [Fact]
    public void Read_ShouldRetry_OnGarbageData()
    {
        var sensor = new SanFengMicrometerSensor("SF01", _runner, _options);

        // Scenario: Garbage -> Garbage -> Success
        _portMock.SetupSequence(p => p.ReadLine())
                 .Returns("garbage")
                 .Returns("???")
                 .Returns("01A-0005.000");
                 
        var result = sensor.Read();
        
        result.IsSucc.Should().BeTrue();
        result.IfSucc(v => v.Should().Be(-5.000));
        
        // Should have retried 3 times (at most), simplified runner logic
        // Verify write called 3 times
        _portMock.Verify(p => p.Write("1"), Times.Exactly(3));
    }
    
    [Fact]
    public void Read_ShouldFail_OnAlarm()
    {
        var sensor = new SanFengMicrometerSensor("SF01", _runner, _options);
        
        // Scenario: Alarm 910
        _portMock.Setup(p => p.ReadLine()).Returns("910");
        
        var result = sensor.Read();
        
        result.IsFail.Should().BeTrue();
        result.IfFail(e => e.Message.Should().Contain("报警码"));
    }
}
