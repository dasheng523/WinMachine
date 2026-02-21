using System;
using System.Text.Json.Serialization;
using LanguageExt;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Core.Types;

/// <summary>可组合实体 ID（newtype 模式）</summary>
public readonly record struct EntityId(Guid Value)
{
    /// <summary>创建新的实体 ID</summary>
    public static EntityId NewId() => new(Guid.NewGuid());
}

/// <summary>组合错误</summary>
public abstract record CompositionError
{
    public sealed record InvalidCoordinate(string Message) : CompositionError;
    public sealed record CircularReference : CompositionError
    {
        private CircularReference() { }
        public static readonly CircularReference Instance = new();
    }
    public sealed record MaxDepthExceeded(int Depth) : CompositionError;
    public sealed record CannotAddChildToLeaf : CompositionError
    {
        private CannotAddChildToLeaf() { }
        public static readonly CannotAddChildToLeaf Instance = new();
    }
    
    private CompositionError() { }
}

/// <summary>可组合实体（递归代数数据类型）</summary>
[JsonDerivedType(typeof(Part), "part")]
[JsonDerivedType(typeof(Composite), "composite")]
public abstract record ComposableEntity
{
    /// <summary>基础零件</summary>
    public sealed record Part(
        EntityId Id,
        Types.Part PartData,
        Coordinate Coordinate,
        PartConfig Config) : ComposableEntity;
    
    /// <summary>组合实体（递归）</summary>
    public sealed record Composite(
        EntityId Id,
        string Name,
        Seq<(ComposableEntity Entity, Coordinate RelativeCoord)> Children,
        Coordinate Coordinate) : ComposableEntity;
    
    private ComposableEntity() { }
    
    /// <summary>获取实体 ID</summary>
    public EntityId GetId() => this switch
    {
        Part p => p.Id,
        Composite c => c.Id,
        _ => throw new InvalidOperationException("Unknown ComposableEntity type")
    };
    
    /// <summary>获取坐标</summary>
    public Coordinate GetCoordinate() => this switch
    {
        Part p => p.Coordinate,
        Composite c => c.Coordinate,
        _ => throw new InvalidOperationException("Unknown ComposableEntity type")
    };
    
    /// <summary>应用变换（递归）</summary>
    /// <remarks>
    /// 对 Part 坐标应用变换，递归地对 Composite 子实体应用变换，保持相对坐标不变。
    /// 验证：需求 3.6-3.7、2.6
    /// </remarks>
    public ComposableEntity ApplyTransformation(TransformationMatrix transform) =>
        this switch
        {
            Part p => p with { Coordinate = transform.ApplyTo(p.Coordinate) },
            Composite c => c with
            {
                Coordinate = transform.ApplyTo(c.Coordinate),
                // 递归应用变换到所有子实体，但保持相对坐标不变
                Children = c.Children.Map(child =>
                    (child.Entity.ApplyTransformation(transform), child.RelativeCoord))
            },
            _ => throw new InvalidOperationException("Unknown ComposableEntity type")
        };
    
    /// <summary>计算所有零件的绝对坐标</summary>
    /// <remarks>
    /// 从根到叶递归计算绝对坐标，组合父子相对坐标，返回 (PartId, Coordinate) 对的序列。
    /// 验证：需求 2.1-2.2、2.9-2.10
    /// </remarks>
    public Seq<(PartId, Coordinate)> ComputeAbsoluteCoordinates() =>
        ComputeAbsoluteCoordinatesHelper(Coordinate.Identity);
    
    /// <summary>计算绝对坐标的辅助方法</summary>
    /// <param name="parentAbsolute">父实体的绝对坐标</param>
    private Seq<(PartId, Coordinate)> ComputeAbsoluteCoordinatesHelper(
        Coordinate parentAbsolute) =>
        this switch
        {
            Part p => Seq1((p.PartData.Id, 
                Domain.CoordinateSystem.ComposeCoordinates(parentAbsolute, p.Coordinate))),
            Composite c => c.Children.Bind(child =>
            {
                // 计算子实体的绝对坐标
                var childAbsolute = Domain.CoordinateSystem.ComposeCoordinates(
                    parentAbsolute, child.RelativeCoord);
                // 递归计算子实体的所有零件坐标
                return child.Entity.ComputeAbsoluteCoordinatesHelper(childAbsolute);
            }),
            _ => Seq<(PartId, Coordinate)>()
        };
    
    /// <summary>添加子实体</summary>
    /// <remarks>
    /// 添加带相对坐标的子实体，返回 Either&lt;CompositionError, ComposableEntity&gt;，验证组合约束。
    /// 验证：需求 3.2、3.4
    /// </remarks>
    public Either<CompositionError, ComposableEntity> AddChild(
        ComposableEntity child,
        Coordinate relativeCoord) =>
        this switch
        {
            Composite c => Right<CompositionError, ComposableEntity>(
                c with
                {
                    Children = c.Children.Add((child, relativeCoord))
                }),
            Part _ => Left<CompositionError, ComposableEntity>(
                CompositionError.CannotAddChildToLeaf.Instance),
            _ => Left<CompositionError, ComposableEntity>(
                new CompositionError.InvalidCoordinate("Unknown entity type"))
        };
}


/// <summary>类型别名和工厂函数（语义清晰）</summary>
/// <remarks>验证：需求 3.1</remarks>
public static class ComposableEntityFactory
{
    /// <summary>创建组件（Component）</summary>
    public static ComposableEntity.Composite CreateComponent(
        EntityId id,
        string name,
        Seq<(ComposableEntity, Coordinate)> children,
        Coordinate coordinate) =>
        new(id, name, children, coordinate);
    
    /// <summary>创建组件（Component）- 使用新 ID</summary>
    public static ComposableEntity.Composite CreateComponent(
        string name,
        Seq<(ComposableEntity, Coordinate)> children,
        Coordinate coordinate) =>
        new(EntityId.NewId(), name, children, coordinate);
    
    /// <summary>创建模组（Module）</summary>
    public static ComposableEntity.Composite CreateModule(
        EntityId id,
        string name,
        Seq<(ComposableEntity, Coordinate)> children,
        Coordinate coordinate) =>
        new(id, name, children, coordinate);
    
    /// <summary>创建模组（Module）- 使用新 ID</summary>
    public static ComposableEntity.Composite CreateModule(
        string name,
        Seq<(ComposableEntity, Coordinate)> children,
        Coordinate coordinate) =>
        new(EntityId.NewId(), name, children, coordinate);
    
    /// <summary>创建机器（Machine）</summary>
    public static ComposableEntity.Composite CreateMachine(
        EntityId id,
        string name,
        Seq<(ComposableEntity, Coordinate)> children,
        Coordinate coordinate) =>
        new(id, name, children, coordinate);
    
    /// <summary>创建机器（Machine）- 使用新 ID</summary>
    public static ComposableEntity.Composite CreateMachine(
        string name,
        Seq<(ComposableEntity, Coordinate)> children,
        Coordinate coordinate) =>
        new(EntityId.NewId(), name, children, coordinate);
}
