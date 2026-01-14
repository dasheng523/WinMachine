using System;
using Devices.Sensors.Core;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Modbus.Device;
using Moq;
using Xunit;
using static LanguageExt.Prelude;

namespace Devices.Tests.Sensors;

public class ModbusOpTests
{
    private readonly Mock<IModbusSerialMaster> _masterMock;

    public ModbusOpTests()
    {
        _masterMock = new Mock<IModbusSerialMaster>();
    }

    [Fact]
    public void SelectMany_ShouldCombineOperations()
    {
        // 模拟一个复杂的事务：
        // 1. 读寄存器 (Length=2)
        // 2. 根据读到的值，写另一个寄存器
        
        var op = from regs in ModbusOp.ReadHoldingRegisters(1, 100, 2)
                 let val = regs[0] + regs[1]
                 from _ in ModbusOp.WriteSingleRegister(1, 200, (ushort)val)
                 select val;

        // Mock behaviors
        _masterMock.Setup(m => m.ReadHoldingRegisters(1, 100, 2))
                   .Returns(new ushort[] { 10, 20 });
                   
        // Execute
        var result = op(_masterMock.Object);
        
        // Assert
        result.IsSucc.Should().BeTrue();
        result.IfSucc(v => v.Should().Be(30));
        
        _masterMock.Verify(m => m.ReadHoldingRegisters(1, 100, 2), Times.Once);
        // Verify dependent write logic
        _masterMock.Verify(m => m.WriteSingleRegister(1, 200, 30), Times.Once);
    }

    [Fact]
    public void Read_ShouldMapExceptionToFail()
    {
        _masterMock.Setup(m => m.ReadHoldingRegisters(It.IsAny<byte>(), It.IsAny<ushort>(), It.IsAny<ushort>()))
                   .Throws(new TimeoutException("Timeout"));

        var op = ModbusOp.ReadHoldingRegisters(1, 100, 1);
        
        var result = op(_masterMock.Object);
        
        result.IsFail.Should().BeTrue();
        result.IfFail(e => e.Message.Should().Contain("Timeout"));
    }
}
