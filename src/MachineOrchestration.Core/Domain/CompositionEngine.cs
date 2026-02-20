using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Core.Domain;

/// <summary>
/// 组合引擎实现（纯函数）
/// 使用 ComposableEntity 的 AddChild 方法和委托模式实现组合操作
/// </summary>
/// <remarks>
/// 验证：需求 3.2-3.7
/// </remarks>
public class CompositionEngine : ICompositionEngine
{
    /// <summary>
    /// 组合两个实体
    /// </summary>
    /// <remarks>
    /// 使用 ComposableEntity.AddChild 方法实现组合
    /// 验证：需求 3.2
    /// </remarks>
    public Either<CompositionError, ComposableEntity> Compose(
        ComposableEntity parent,
        ComposableEntity child,
        Coordinate relativeCoord)
    {
        // 委托给 ComposableEntity 的 AddChild 方法
        return parent.AddChild(child, relativeCoord);
    }
    
    /// <summary>
    /// 应用变换到实体
    /// </summary>
    /// <remarks>
    /// 委托给 ComposableEntity 的 ApplyTransformation 方法
    /// 验证：需求 3.6-3.7
    /// </remarks>
    public ComposableEntity ApplyTransformation(
        ComposableEntity entity,
        TransformationMatrix transform)
    {
        // 委托给实体的 ApplyTransformation 方法
        return entity.ApplyTransformation(transform);
    }
    
    /// <summary>
    /// 计算实体中所有零件的绝对坐标
    /// </summary>
    /// <remarks>
    /// 委托给 ComposableEntity 的 ComputeAbsoluteCoordinates 方法
    /// 验证：需求 3.6-3.7
    /// </remarks>
    public Seq<(PartId, Coordinate)> ComputeAbsoluteCoordinates(
        ComposableEntity entity)
    {
        // 委托给实体的 ComputeAbsoluteCoordinates 方法
        return entity.ComputeAbsoluteCoordinates();
    }
}
