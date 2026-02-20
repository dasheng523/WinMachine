using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Core.Domain;

/// <summary>
/// 组合引擎接口（纯函数）
/// 处理零件的递归组合和坐标变换
/// </summary>
/// <remarks>
/// 验证：需求 3.2, 3.6-3.7
/// </remarks>
public interface ICompositionEngine
{
    /// <summary>
    /// 组合两个实体
    /// </summary>
    /// <param name="parent">父实体</param>
    /// <param name="child">子实体</param>
    /// <param name="relativeCoord">子实体相对于父实体的坐标</param>
    /// <returns>组合后的实体或组合错误</returns>
    /// <remarks>
    /// 验证：需求 3.2
    /// </remarks>
    Either<CompositionError, ComposableEntity> Compose(
        ComposableEntity parent,
        ComposableEntity child,
        Coordinate relativeCoord);
    
    /// <summary>
    /// 应用变换到实体
    /// </summary>
    /// <param name="entity">要变换的实体</param>
    /// <param name="transform">变换矩阵</param>
    /// <returns>变换后的实体</returns>
    /// <remarks>
    /// 验证：需求 3.6-3.7
    /// </remarks>
    ComposableEntity ApplyTransformation(
        ComposableEntity entity,
        TransformationMatrix transform);
    
    /// <summary>
    /// 计算实体中所有零件的绝对坐标
    /// </summary>
    /// <param name="entity">要计算的实体</param>
    /// <returns>零件ID和绝对坐标的序列</returns>
    /// <remarks>
    /// 验证：需求 3.6-3.7
    /// </remarks>
    Seq<(PartId, Coordinate)> ComputeAbsoluteCoordinates(
        ComposableEntity entity);
}
