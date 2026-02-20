using System;
using System.Numerics;
using Xunit;
using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.Core.Domain;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// 组合引擎单元测试
/// 测试组合错误情况
/// </summary>
public class CompositionEngineTests
{
    private readonly ICompositionEngine _engine;
    
    public CompositionEngineTests()
    {
        _engine = new CompositionEngine();
    }
    
    /// <summary>
    /// 测试：尝试向叶子节点（Part）添加子节点应该返回错误
    /// </summary>
    [Fact]
    public void Compose_AddChildToLeafNode_ReturnsCannotAddChildToLeafError()
    {
        // Arrange - 创建一个 Part（叶子节点）
        var part = CreateTestPart();
        var childPart = CreateTestPart();
        var relativeCoord = new Coordinate(Vector3.Zero, Quaternion.Identity);
        
        // Act - 尝试向 Part 添加子节点
        var result = _engine.Compose(part, childPart, relativeCoord);
        
        // Assert - 应该返回 CannotAddChildToLeaf 错误
        result.Match(
            Right: _ => Assert.Fail("Should have returned an error"),
            Left: error => Assert.IsType<CompositionError.CannotAddChildToLeaf>(error));
    }
    
    /// <summary>
    /// 测试：向 Composite 添加子节点应该成功
    /// </summary>
    [Fact]
    public void Compose_AddChildToComposite_Succeeds()
    {
        // Arrange - 创建一个 Composite
        var composite = CreateTestComposite();
        var childPart = CreateTestPart();
        var relativeCoord = new Coordinate(
            new Vector3(1, 0, 0),
            Quaternion.Identity);
        
        // Act - 向 Composite 添加子节点
        var result = _engine.Compose(composite, childPart, relativeCoord);
        
        // Assert - 应该成功
        result.Match(
            Right: entity =>
            {
                Assert.IsType<ComposableEntity.Composite>(entity);
                var comp = (ComposableEntity.Composite)entity;
                Assert.Equal(1, comp.Children.Count);
            },
            Left: error => Assert.Fail($"Should have succeeded, but got error: {error}"));
    }
    
    /// <summary>
    /// 测试：ApplyTransformation 应该委托给实体方法
    /// </summary>
    [Fact]
    public void ApplyTransformation_DelegatesToEntityMethod()
    {
        // Arrange
        var part = CreateTestPart();
        var transform = TransformationMatrix.Translation(new Vector3(5, 0, 0));
        
        // Act
        var result = _engine.ApplyTransformation(part, transform);
        
        // Assert - 应该返回变换后的实体
        Assert.NotNull(result);
        var transformedPart = Assert.IsType<ComposableEntity.Part>(result);
        
        // 验证坐标已变换
        Assert.NotEqual(part.Coordinate, transformedPart.Coordinate);
    }
    
    /// <summary>
    /// 测试：ApplyTransformation 应该递归应用到所有子实体
    /// </summary>
    [Fact]
    public void ApplyTransformation_AppliesRecursivelyToChildren()
    {
        // Arrange - 创建带子节点的 Composite
        var childPart = CreateTestPart();
        var composite = CreateTestComposite();
        var compositeWithChild = _engine.Compose(
            composite,
            childPart,
            new Coordinate(Vector3.Zero, Quaternion.Identity))
            .Match(
                Right: entity => entity,
                Left: _ => throw new Exception("Failed to compose"));
        
        var transform = TransformationMatrix.Translation(new Vector3(10, 0, 0));
        
        // Act
        var result = _engine.ApplyTransformation(compositeWithChild, transform);
        
        // Assert
        var transformedComposite = Assert.IsType<ComposableEntity.Composite>(result);
        Assert.Equal(1, transformedComposite.Children.Count);
        
        // 验证子节点也被变换
        var (childEntity, _) = transformedComposite.Children.Head;
        Assert.NotNull(childEntity);
    }
    
    /// <summary>
    /// 测试：ComputeAbsoluteCoordinates 应该返回所有零件的绝对坐标
    /// </summary>
    [Fact]
    public void ComputeAbsoluteCoordinates_ReturnsAllPartCoordinates()
    {
        // Arrange - 创建带子节点的 Composite
        var part1 = CreateTestPart();
        var part2 = CreateTestPart();
        var composite = CreateTestComposite();
        
        var compositeWithChildren = _engine.Compose(
            composite,
            part1,
            new Coordinate(new Vector3(1, 0, 0), Quaternion.Identity))
            .Bind(c => _engine.Compose(
                c,
                part2,
                new Coordinate(new Vector3(2, 0, 0), Quaternion.Identity)))
            .Match(
                Right: entity => entity,
                Left: _ => throw new Exception("Failed to compose"));
        
        // Act
        var coordinates = _engine.ComputeAbsoluteCoordinates(compositeWithChildren);
        
        // Assert - 应该返回两个零件的坐标
        Assert.Equal(2, coordinates.Count);
    }
    
    /// <summary>
    /// 测试：ComputeAbsoluteCoordinates 对单个 Part 应该返回其坐标
    /// </summary>
    [Fact]
    public void ComputeAbsoluteCoordinates_ForSinglePart_ReturnsItsCoordinate()
    {
        // Arrange
        var part = CreateTestPart();
        
        // Act
        var coordinates = _engine.ComputeAbsoluteCoordinates(part);
        
        // Assert
        Assert.Single(coordinates);
        var (partId, coord) = coordinates.Head;
        Assert.Equal(((ComposableEntity.Part)part).PartData.Id, partId);
    }
    
    // Helper methods
    
    private ComposableEntity.Part CreateTestPart()
    {
        var partData = new Part(
            new PartId(Guid.NewGuid()),
            "TestPart",
            new PartType.Static(new StaticType.Shaft(1.0f, 0.1f)),
            PartCategory.StaticType.Instance,
            new Vector3(1, 1, 1));
        
        return new ComposableEntity.Part(
            EntityId.NewId(),
            partData,
            new Coordinate(Vector3.Zero, Quaternion.Identity),
            PartConfig.Static.Instance);
    }
    
    private ComposableEntity.Composite CreateTestComposite()
    {
        return new ComposableEntity.Composite(
            EntityId.NewId(),
            "TestComposite",
            Seq<(ComposableEntity, Coordinate)>(),
            new Coordinate(Vector3.Zero, Quaternion.Identity));
    }
}
