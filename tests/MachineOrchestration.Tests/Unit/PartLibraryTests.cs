using System;
using LanguageExt;
using MachineOrchestration.Core.Domain;
using MachineOrchestration.Core.Types;
using Xunit;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// 零件库单元测试
/// </summary>
public class PartLibraryTests
{
    private readonly IPartLibrary _partLibrary = PartLibrary.Instance;
    
    // GetPartsByCategory 测试
    
    [Fact]
    public void GetPartsByCategory_MotorType_ReturnsOnlyMotorParts()
    {
        // Act
        var motorParts = _partLibrary.GetPartsByCategory(PartCategory.MotorType.Instance);
        
        // Assert
        Assert.NotEmpty(motorParts);
        Assert.True(motorParts.ForAll(part => part.Category is PartCategory.MotorType));
        Assert.True(motorParts.ForAll(part => part.PartType is PartType.Motor));
    }
    
    [Fact]
    public void GetPartsByCategory_OutputType_ReturnsOnlyOutputParts()
    {
        // Act
        var outputParts = _partLibrary.GetPartsByCategory(PartCategory.OutputType.Instance);
        
        // Assert
        Assert.NotEmpty(outputParts);
        Assert.True(outputParts.ForAll(part => part.Category is PartCategory.OutputType));
        Assert.True(outputParts.ForAll(part => part.PartType is PartType.Actuator));
    }
    
    [Fact]
    public void GetPartsByCategory_InputType_ReturnsOnlySensorParts()
    {
        // Act
        var inputParts = _partLibrary.GetPartsByCategory(PartCategory.InputType.Instance);
        
        // Assert
        Assert.NotEmpty(inputParts);
        Assert.True(inputParts.ForAll(part => part.Category is PartCategory.InputType));
        Assert.True(inputParts.ForAll(part => part.PartType is PartType.Sensor));
    }
    
    [Fact]
    public void GetPartsByCategory_StaticType_ReturnsOnlyStaticParts()
    {
        // Act
        var staticParts = _partLibrary.GetPartsByCategory(PartCategory.StaticType.Instance);
        
        // Assert
        Assert.NotEmpty(staticParts);
        Assert.True(staticParts.ForAll(part => part.Category is PartCategory.StaticType));
        Assert.True(staticParts.ForAll(part => part.PartType is PartType.Static));
    }
    
    // GetPartById 测试
    
    [Fact]
    public void GetPartById_WithValidId_ReturnsSomePart()
    {
        // Arrange
        var allParts = _partLibrary.GetAllParts();
        var firstPart = allParts.Head;
        
        // Act
        var result = _partLibrary.GetPartById(firstPart.Id);
        
        // Assert
        Assert.True(result.IsSome);
        result.IfSome(part =>
        {
            Assert.Equal(firstPart.Id, part.Id);
            Assert.Equal(firstPart.Name, part.Name);
        });
    }
    
    [Fact]
    public void GetPartById_WithInvalidId_ReturnsNone()
    {
        // Arrange
        var invalidId = new PartId(Guid.NewGuid());
        
        // Act
        var result = _partLibrary.GetPartById(invalidId);
        
        // Assert
        Assert.True(result.IsNone);
    }
    
    [Fact]
    public void GetPartById_WithKnownMotorId_ReturnsCorrectMotor()
    {
        // Arrange - 丝杆滑块电机的 ID
        var linearScrewId = new PartId(Guid.Parse("10000000-0000-0000-0000-000000000001"));
        
        // Act
        var result = _partLibrary.GetPartById(linearScrewId);
        
        // Assert
        Assert.True(result.IsSome);
        result.IfSome(part =>
        {
            Assert.Equal("丝杆滑块电机", part.Name);
            Assert.IsType<PartType.Motor>(part.PartType);
            Assert.IsType<PartCategory.MotorType>(part.Category);
        });
    }
    
    [Fact]
    public void GetPartById_WithKnownCylinderId_ReturnsCorrectCylinder()
    {
        // Arrange - 气缸的 ID
        var cylinderId = new PartId(Guid.Parse("20000000-0000-0000-0000-000000000001"));
        
        // Act
        var result = _partLibrary.GetPartById(cylinderId);
        
        // Assert
        Assert.True(result.IsSome);
        result.IfSome(part =>
        {
            Assert.Equal("气缸", part.Name);
            Assert.IsType<PartType.Actuator>(part.PartType);
            Assert.IsType<PartCategory.OutputType>(part.Category);
        });
    }
    
    // GetAllParts 测试
    
    [Fact]
    public void GetAllParts_ReturnsNonEmptyCollection()
    {
        // Act
        var allParts = _partLibrary.GetAllParts();
        
        // Assert
        Assert.NotEmpty(allParts);
    }
    
    [Fact]
    public void GetAllParts_ContainsExpectedNumberOfParts()
    {
        // Act
        var allParts = _partLibrary.GetAllParts();
        
        // Assert
        // 应该有：2个电机 + 4个执行器 + 3个传感器 + 2个静态零件 = 11个零件
        Assert.Equal(11, allParts.Count);
    }
    
    [Fact]
    public void GetAllParts_ContainsAllCategories()
    {
        // Act
        var allParts = _partLibrary.GetAllParts();
        
        // Assert
        Assert.Contains(allParts, p => p.Category is PartCategory.MotorType);
        Assert.Contains(allParts, p => p.Category is PartCategory.OutputType);
        Assert.Contains(allParts, p => p.Category is PartCategory.InputType);
        Assert.Contains(allParts, p => p.Category is PartCategory.StaticType);
    }
    
    [Fact]
    public void GetAllParts_AllPartsHaveUniqueIds()
    {
        // Act
        var allParts = _partLibrary.GetAllParts();
        var uniqueIds = allParts.Map(p => p.Id).Distinct();
        
        // Assert
        Assert.Equal(allParts.Count, uniqueIds.Count());
    }
    
    [Fact]
    public void GetAllParts_AllPartsHaveValidDimensions()
    {
        // Act
        var allParts = _partLibrary.GetAllParts();
        
        // Assert
        Assert.True(allParts.ForAll(part =>
            part.PhysicalDimensions.X > 0 &&
            part.PhysicalDimensions.Y > 0 &&
            part.PhysicalDimensions.Z > 0));
    }
}
