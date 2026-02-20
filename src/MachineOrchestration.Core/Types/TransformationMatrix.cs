using System.Numerics;

namespace MachineOrchestration.Core.Types;

/// <summary>变换矩阵（4x4 齐次变换矩阵）</summary>
public readonly record struct TransformationMatrix
{
    private readonly Matrix4x4 _matrix;
    
    private TransformationMatrix(Matrix4x4 matrix) => _matrix = matrix;
    
    /// <summary>单位变换矩阵</summary>
    public static readonly TransformationMatrix Identity = 
        new(Matrix4x4.Identity);
    
    /// <summary>创建平移变换</summary>
    public static TransformationMatrix Translation(Vector3 v) =>
        new(Matrix4x4.CreateTranslation(v));
    
    /// <summary>创建旋转变换</summary>
    public static TransformationMatrix Rotation(Quaternion q) =>
        new(Matrix4x4.CreateFromQuaternion(q));
    
    /// <summary>创建缩放变换</summary>
    public static TransformationMatrix Scale(Vector3 s) =>
        new(Matrix4x4.CreateScale(s));
    
    /// <summary>创建完整变换（缩放 → 旋转 → 平移）</summary>
    public static TransformationMatrix Create(
        Vector3 translation,
        Quaternion rotation,
        Vector3 scale) =>
        new(Matrix4x4.CreateTranslation(translation) *
            Matrix4x4.CreateFromQuaternion(rotation) *
            Matrix4x4.CreateScale(scale));
    
    /// <summary>组合变换（满足结合律）</summary>
    public TransformationMatrix Compose(TransformationMatrix other) =>
        new(_matrix * other._matrix);
    
    /// <summary>应用到坐标</summary>
    public Coordinate ApplyTo(Coordinate coord)
    {
        // 变换位置
        var transformedPos = Vector3.Transform(coord.Position, _matrix);
        
        // 提取旋转矩阵并组合旋转
        var rotationMatrix = Matrix4x4.CreateFromQuaternion(coord.Rotation);
        var combinedMatrix = rotationMatrix * _matrix;
        
        // 从组合矩阵中提取旋转（使用 Quaternion.CreateFromRotationMatrix）
        // 注意：需要移除平移和缩放分量
        var scaleX = new Vector3(combinedMatrix.M11, combinedMatrix.M12, combinedMatrix.M13).Length();
        var scaleY = new Vector3(combinedMatrix.M21, combinedMatrix.M22, combinedMatrix.M23).Length();
        var scaleZ = new Vector3(combinedMatrix.M31, combinedMatrix.M32, combinedMatrix.M33).Length();
        
        var pureRotationMatrix = new Matrix4x4(
            combinedMatrix.M11 / scaleX, combinedMatrix.M12 / scaleX, combinedMatrix.M13 / scaleX, 0,
            combinedMatrix.M21 / scaleY, combinedMatrix.M22 / scaleY, combinedMatrix.M23 / scaleY, 0,
            combinedMatrix.M31 / scaleZ, combinedMatrix.M32 / scaleZ, combinedMatrix.M33 / scaleZ, 0,
            0, 0, 0, 1
        );
        
        var transformedRot = Quaternion.CreateFromRotationMatrix(pureRotationMatrix);
        
        return new Coordinate(transformedPos, transformedRot);
    }
    
    /// <summary>获取内部矩阵（用于测试和调试）</summary>
    public Matrix4x4 GetMatrix() => _matrix;
}
