using System;
using Common.Hardware;
using Devices.Sensors.Core;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Moq;
using Xunit;
using static LanguageExt.Prelude;

namespace Devices.Tests.Sensors;

public class SerialOpTests
{
    private readonly Mock<ITextLinePort> _portMock;

    public SerialOpTests()
    {
        _portMock = new Mock<ITextLinePort>();
    }

    [Fact]
    public void Bind_ShouldChainOperations()
    {
        // 验证 Monad 的 Bind 能力: Write -> Read -> Map
        var op = from _ in SerialOp.Write("CMD")
                 from res in SerialOp.ReadLine()
                 select res.Length;
                 
        // Mock port behaviors
        // 1. Write should be called
        // 2. ReadLine should return "123"
        _portMock.Setup(p => p.ReadLine()).Returns("123");
        
        // Execute (Interpret)
        var result = op(_portMock.Object);
        
        // Assert
        result.IsSucc.Should().BeTrue();
        result.IfSucc(len => len.Should().Be(3)); // "123".Length
        
        _portMock.Verify(p => p.Write("CMD"), Times.Once);
        _portMock.Verify(p => p.ReadLine(), Times.Once);
    }
    
    [Fact]
    public void Fail_ShouldShortCircuit()
    {
        // 验证失败短路: Write -> Fail -> Read (Should cover)
        var op = from _ in SerialOp.Write("CMD")
                 from f in SerialOp.Fail<string>(Error.New("Boom"))
                 from res in SerialOp.ReadLine()
                 select res;
                 
        var result = op(_portMock.Object);
        
        result.IsFail.Should().BeTrue();
        result.IfFail(err => err.Message.Should().Be("Boom"));
        
        _portMock.Verify(p => p.Write("CMD"), Times.Once);
        _portMock.Verify(p => p.ReadLine(), Times.Never); // Should not be called
    }
    
    [Fact]
    public void Exception_ShouldBeCaughtAsFail()
    {
        // 验证异常安全: Write throws -> Fail
        _portMock.Setup(p => p.Write(It.IsAny<string>())).Throws(new Exception("IO Error"));
        
        var op = SerialOp.Write("CMD");
        
        var result = op(_portMock.Object);
        
        result.IsFail.Should().BeTrue();
        result.IfFail(err => err.Message.Should().Contain("IO Error"));
    }
}
