using System.Numerics;
using Xunit;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Tests.Properties;

/// <summary>
/// 变换矩阵属性测试
/// Feature: machine-orchestration-system
/// </summary>
public class TransformationMatrixProperties
{
    /// <summary>
    /// Property 3: 变换矩阵结合律
    /// 验证：需求 2.3-2.5
    /// 
    /// 对于任意三个变换矩阵 T1、T2、T3，组合操作应该满足结合律：
    /// (T1 ⊕ T2) ⊕ T3 = T1 ⊕ (T2 ⊕ T3)
    /// </summary>
    [Theory]
    [MemberData(nameof(GetTransformationMatrixTriples))]
    public void TransformationMatrix_Compose_IsAssociative(
        TransformationMatrix t1,
        TransformationMatrix t2,
        TransformationMatrix t3)
    {
        // (T1 ⊕ T2) ⊕ T3
        var left = t1.Compose(t2).Compose(t3);
        
        // T1 ⊕ (T2 ⊕ T3)
        var right = t1.Compose(t2.Compose(t3));
        
        // 由于浮点数精度问题，我们需要使用近似相等比较
        // 包含缩放的变换矩阵组合会累积更多的浮点误差
        Assert.True(MatricesAreApproximatelyEqual(left, right, 0.01f),
            "Transformation matrix composition should be associative");
    }
    
    /// <summary>
    /// Property 4: 变换矩阵幺元
    /// 验证：需求 2.3-2.5
    /// 
    /// 对于任意变换矩阵 T，存在单位变换 I，使得：
    /// T ⊕ I = I ⊕ T = T
    /// </summary>
    [Theory]
    [MemberData(nameof(GetTransformationMatrices))]
    public void TransformationMatrix_Identity_IsNeutralElement(TransformationMatrix t)
    {
        // T ⊕ I
        var leftIdentity = t.Compose(TransformationMatrix.Identity);
        
        // I ⊕ T
        var rightIdentity = TransformationMatrix.Identity.Compose(t);
        
        // 验证 T ⊕ I = T 和 I ⊕ T = T
        Assert.True(MatricesAreApproximatelyEqual(leftIdentity, t, 0.0001f),
            "T ⊕ I should equal T");
        Assert.True(MatricesAreApproximatelyEqual(rightIdentity, t, 0.0001f),
            "I ⊕ T should equal T");
    }
    
    /// <summary>比较两个变换矩阵是否近似相等</summary>
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
    
    /// <summary>生成测试用的变换矩阵</summary>
    public static IEnumerable<object[]> GetTransformationMatrices()
    {
        var random = new Random(42); // 固定种子以确保可重复性
        
        for (int i = 0; i < 100; i++)
        {
            yield return new object[] { CreateRandomTransformationMatrix(random) };
        }
    }
    
    /// <summary>生成测试用的变换矩阵三元组</summary>
    public static IEnumerable<object[]> GetTransformationMatrixTriples()
    {
        var random = new Random(42); // 固定种子以确保可重复性
        
        for (int i = 0; i < 100; i++)
        {
            yield return new object[]
            {
                CreateRandomTransformationMatrix(random),
                CreateRandomTransformationMatrix(random),
                CreateRandomTransformationMatrix(random)
            };
        }
    }
    
    private static TransformationMatrix CreateRandomTransformationMatrix(Random rnd)
    {
        var tx = (float)(rnd.NextDouble() * 200 - 100);
        var ty = (float)(rnd.NextDouble() * 200 - 100);
        var tz = (float)(rnd.NextDouble() * 200 - 100);
        
        var qx = (float)(rnd.NextDouble() * 2 - 1);
        var qy = (float)(rnd.NextDouble() * 2 - 1);
        var qz = (float)(rnd.NextDouble() * 2 - 1);
        var qw = (float)(rnd.NextDouble() * 2 - 1);
        
        var sx = (float)(rnd.NextDouble() * 9.9 + 0.1);
        var sy = (float)(rnd.NextDouble() * 9.9 + 0.1);
        var sz = (float)(rnd.NextDouble() * 9.9 + 0.1);
        
        var translation = new Vector3(tx, ty, tz);
        var rotation = Quaternion.Normalize(new Quaternion(qx, qy, qz, qw));
        var scale = new Vector3(sx, sy, sz);
        
        return TransformationMatrix.Create(translation, rotation, scale);
    }
}
