using System.Numerics;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Core.Domain;

/// <summary>坐标系统（纯函数接口）</summary>
public static class CoordinateSystem
{
    /// <summary>创建坐标</summary>
    public static Coordinate CreateCoordinate(
        Vector3 position,
        Quaternion rotation) => 
        new(position, rotation);
    
    /// <summary>组合坐标（相对坐标转绝对坐标）</summary>
    /// <param name="parent">父坐标（绝对坐标）</param>
    /// <param name="childRelative">子坐标（相对于父坐标）</param>
    /// <returns>子坐标的绝对坐标</returns>
    public static Coordinate ComposeCoordinates(
        Coordinate parent,
        Coordinate childRelative)
    {
        // 1. 旋转子坐标的位置向量
        var rotatedPosition = Vector3.Transform(childRelative.Position, parent.Rotation);
        
        // 2. 加上父坐标的位置
        var absolutePosition = parent.Position + rotatedPosition;
        
        // 3. 组合旋转（四元数乘法）
        var absoluteRotation = parent.Rotation * childRelative.Rotation;
        
        return new Coordinate(absolutePosition, absoluteRotation);
    }
    
    /// <summary>创建变换矩阵</summary>
    public static TransformationMatrix CreateTransformation(
        Vector3 translation,
        Quaternion rotation,
        Vector3 scale) =>
        TransformationMatrix.Create(translation, rotation, scale);
    
    /// <summary>组合变换矩阵</summary>
    public static TransformationMatrix ComposeTransformations(
        TransformationMatrix t1,
        TransformationMatrix t2) =>
        t1.Compose(t2);
    
    /// <summary>应用变换到坐标</summary>
    public static Coordinate ApplyToCoordinate(
        TransformationMatrix transform,
        Coordinate coord) =>
        transform.ApplyTo(coord);
}
