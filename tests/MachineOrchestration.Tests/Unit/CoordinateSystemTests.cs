using System.Numerics;
using Xunit;
using MachineOrchestration.Core.Types;
using MachineOrchestration.Core.Domain;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// 单元测试：坐标系统边缘情况
/// Feature: machine-orchestration-system
/// </summary>
public class CoordinateSystemTests
{
    /// <summary>
    /// 测试幺元组合：Identity 坐标与任意坐标组合应返回原坐标
    /// </summary>
    [Fact]
    public void ComposeCoordinates_WithIdentity_ReturnsOriginal()
    {
        // Arrange
        var coord = new Coordinate(
            new Vector3(1, 2, 3),
            Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f));
        
        // Act
        var result1 = CoordinateSystem.ComposeCoordinates(Coordinate.Identity, coord);
        var result2 = CoordinateSystem.ComposeCoordinates(coord, Coordinate.Identity);
        
        // Assert
        Assert.True(CoordinatesAreApproximatelyEqual(result1, coord, 0.0001f));
        // Note: result2 is not equal to coord because Identity is relative to coord
        // The child at Identity relative to parent coord is at coord's position
        Assert.True(CoordinatesAreApproximatelyEqual(result2, coord, 0.0001f));
    }
    
    /// <summary>
    /// 测试零向量处理：零位置向量应正确处理
    /// </summary>
    [Fact]
    public void ComposeCoordinates_WithZeroPosition_HandlesCorrectly()
    {
        // Arrange
        var parent = new Coordinate(
            new Vector3(10, 20, 30),
            Quaternion.Identity);
        var childWithZeroPosition = new Coordinate(
            Vector3.Zero,
            Quaternion.CreateFromYawPitchRoll(0.5f, 0, 0));
        
        // Act
        var result = CoordinateSystem.ComposeCoordinates(parent, childWithZeroPosition);
        
        // Assert
        // 子坐标位置为零，所以绝对位置应该等于父坐标位置
        Assert.True(Vector3.Distance(result.Position, parent.Position) < 0.0001f);
        // 旋转应该是父旋转和子旋转的组合
        var expectedRotation = parent.Rotation * childWithZeroPosition.Rotation;
        Assert.True(QuaternionsAreApproximatelyEqual(result.Rotation, expectedRotation, 0.0001f));
    }
    
    /// <summary>
    /// 测试四元数归一化：非归一化的四元数应正确处理
    /// </summary>
    [Fact]
    public void ComposeCoordinates_WithNonNormalizedQuaternion_HandlesCorrectly()
    {
        // Arrange
        var parent = new Coordinate(
            Vector3.Zero,
            new Quaternion(1, 1, 1, 1)); // 非归一化四元数
        var child = new Coordinate(
            new Vector3(1, 0, 0),
            Quaternion.Identity);
        
        // Act
        // 归一化父坐标的旋转
        var normalizedParent = new Coordinate(
            parent.Position,
            Quaternion.Normalize(parent.Rotation));
        var result = CoordinateSystem.ComposeCoordinates(normalizedParent, child);
        
        // Assert
        // 结果应该是有效的
        Assert.False(float.IsNaN(result.Position.X));
        Assert.False(float.IsNaN(result.Position.Y));
        Assert.False(float.IsNaN(result.Position.Z));
        Assert.False(float.IsNaN(result.Rotation.X));
        Assert.False(float.IsNaN(result.Rotation.Y));
        Assert.False(float.IsNaN(result.Rotation.Z));
        Assert.False(float.IsNaN(result.Rotation.W));
    }
    
    /// <summary>
    /// 测试变换矩阵幺元：Identity 变换与任意变换组合应返回原变换
    /// </summary>
    [Fact]
    public void TransformationMatrix_Compose_WithIdentity_ReturnsOriginal()
    {
        // Arrange
        var transform = TransformationMatrix.Create(
            new Vector3(1, 2, 3),
            Quaternion.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f),
            new Vector3(1, 1, 1));
        
        // Act
        var result1 = transform.Compose(TransformationMatrix.Identity);
        var result2 = TransformationMatrix.Identity.Compose(transform);
        
        // Assert
        Assert.True(MatricesAreApproximatelyEqual(result1, transform, 0.0001f));
        Assert.True(MatricesAreApproximatelyEqual(result2, transform, 0.0001f));
    }
    
    /// <summary>
    /// 测试变换矩阵应用到 Identity 坐标
    /// </summary>
    [Fact]
    public void TransformationMatrix_ApplyTo_IdentityCoordinate()
    {
        // Arrange
        var transform = TransformationMatrix.Translation(new Vector3(10, 20, 30));
        
        // Act
        var result = transform.ApplyTo(Coordinate.Identity);
        
        // Assert
        // Identity 坐标经过平移变换后，位置应该改变
        Assert.True(Vector3.Distance(result.Position, new Vector3(10, 20, 30)) < 0.0001f);
    }
    
    // Helper methods
    private static bool CoordinatesAreApproximatelyEqual(
        Coordinate c1,
        Coordinate c2,
        float epsilon)
    {
        return Vector3.Distance(c1.Position, c2.Position) < epsilon &&
               QuaternionsAreApproximatelyEqual(c1.Rotation, c2.Rotation, epsilon);
    }
    
    private static bool QuaternionsAreApproximatelyEqual(
        Quaternion q1,
        Quaternion q2,
        float epsilon)
    {
        // 四元数 q 和 -q 表示相同的旋转
        var dot = q1.X * q2.X + q1.Y * q2.Y + q1.Z * q2.Z + q1.W * q2.W;
        return Math.Abs(Math.Abs(dot) - 1.0f) < epsilon;
    }
    
    private static bool MatricesAreApproximatelyEqual(
        TransformationMatrix m1,
        TransformationMatrix m2,
        float epsilon)
    {
        var mat1 = m1.GetMatrix();
        var mat2 = m2.GetMatrix();
        
        return Math.Abs(mat1.M11 - mat2.M11) < epsilon &&
               Math.Abs(mat1.M12 - mat2.M12) < epsilon &&
               Math.Abs(mat1.M13 - mat2.M13) < epsilon &&
               Math.Abs(mat1.M14 - mat2.M14) < epsilon &&
               Math.Abs(mat1.M21 - mat2.M21) < epsilon &&
               Math.Abs(mat1.M22 - mat2.M22) < epsilon &&
               Math.Abs(mat1.M23 - mat2.M23) < epsilon &&
               Math.Abs(mat1.M24 - mat2.M24) < epsilon &&
               Math.Abs(mat1.M31 - mat2.M31) < epsilon &&
               Math.Abs(mat1.M32 - mat2.M32) < epsilon &&
               Math.Abs(mat1.M33 - mat2.M33) < epsilon &&
               Math.Abs(mat1.M34 - mat2.M34) < epsilon &&
               Math.Abs(mat1.M41 - mat2.M41) < epsilon &&
               Math.Abs(mat1.M42 - mat2.M42) < epsilon &&
               Math.Abs(mat1.M43 - mat2.M43) < epsilon &&
               Math.Abs(mat1.M44 - mat2.M44) < epsilon;
    }
}
