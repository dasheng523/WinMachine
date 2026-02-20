using LanguageExt;
using MachineOrchestration.Core.Domain;
using MachineOrchestration.Core.Types;
using Xunit;

namespace MachineOrchestration.Tests.Properties;

/// <summary>
/// 零件库属性测试
/// Feature: machine-orchestration-system
/// </summary>
public class PartLibraryProperties
{
    private readonly IPartLibrary _partLibrary = PartLibrary.Instance;
    
    /// <summary>
    /// Property 2: 零件分类查询一致性
    /// 验证：需求 1.14-1.15
    /// 
    /// 对于任意零件分类和零件库，按分类查询返回的所有零件都应该属于该分类，
    /// 且该分类的所有零件都应该被返回。
    /// </summary>
    [Theory]
    [MemberData(nameof(GetAllCategories))]
    public void PartLibrary_GetPartsByCategory_ReturnsOnlyPartsOfThatCategory(PartCategory category)
    {
        // 按分类获取零件
        var partsInCategory = _partLibrary.GetPartsByCategory(category);
        
        // 检查1：返回的所有零件都应该属于该分类
        var allPartsMatchCategory = partsInCategory.ForAll(part =>
            part.Category.GetType() == category.GetType());
        
        // 检查2：该分类的所有零件都应该被返回
        var allParts = _partLibrary.GetAllParts();
        var expectedPartsInCategory = allParts.Filter(part =>
            part.Category.GetType() == category.GetType());
        
        var allExpectedPartsReturned = expectedPartsInCategory.ForAll(expectedPart =>
            partsInCategory.Exists(part => part.Id.Equals(expectedPart.Id)));
        
        // 检查3：返回的零件数量应该等于预期数量
        var countMatches = partsInCategory.Count == expectedPartsInCategory.Count;
        
        Assert.True(allPartsMatchCategory, 
            $"All parts in category {category.GetType().Name} should match the category");
        Assert.True(allExpectedPartsReturned, 
            $"All expected parts in category {category.GetType().Name} should be returned");
        Assert.True(countMatches, 
            $"Count of parts in category {category.GetType().Name} should match expected count");
    }
    
    /// <summary>生成所有零件分类</summary>
    public static IEnumerable<object[]> GetAllCategories()
    {
        yield return new object[] { PartCategory.MotorType.Instance };
        yield return new object[] { PartCategory.OutputType.Instance };
        yield return new object[] { PartCategory.InputType.Instance };
        yield return new object[] { PartCategory.StaticType.Instance };
    }
    
    /// <summary>
    /// 零件分类查询应该是完整的：所有零件的并集应该等于全部零件
    /// </summary>
    [Fact]
    public void PartLibrary_AllCategories_ShouldCoverAllParts()
    {
        var allParts = _partLibrary.GetAllParts();
        
        var motorParts = _partLibrary.GetPartsByCategory(PartCategory.MotorType.Instance);
        var outputParts = _partLibrary.GetPartsByCategory(PartCategory.OutputType.Instance);
        var inputParts = _partLibrary.GetPartsByCategory(PartCategory.InputType.Instance);
        var staticParts = _partLibrary.GetPartsByCategory(PartCategory.StaticType.Instance);
        
        var unionOfCategories = motorParts
            .Append(outputParts)
            .Append(inputParts)
            .Append(staticParts);
        
        // 所有分类的并集应该包含所有零件
        Assert.Equal(allParts.Count, unionOfCategories.Count);
        
        // 每个零件都应该在某个分类中
        Assert.True(allParts.ForAll(part =>
            unionOfCategories.Exists(p => p.Id.Equals(part.Id))));
    }
    
    /// <summary>
    /// 零件分类应该是互斥的：没有零件应该属于多个分类
    /// </summary>
    [Fact]
    public void PartLibrary_Categories_ShouldBeMutuallyExclusive()
    {
        var motorParts = _partLibrary.GetPartsByCategory(PartCategory.MotorType.Instance);
        var outputParts = _partLibrary.GetPartsByCategory(PartCategory.OutputType.Instance);
        var inputParts = _partLibrary.GetPartsByCategory(PartCategory.InputType.Instance);
        var staticParts = _partLibrary.GetPartsByCategory(PartCategory.StaticType.Instance);
        
        // 检查每对分类之间没有交集
        Assert.Empty(motorParts.Intersect(outputParts));
        Assert.Empty(motorParts.Intersect(inputParts));
        Assert.Empty(motorParts.Intersect(staticParts));
        Assert.Empty(outputParts.Intersect(inputParts));
        Assert.Empty(outputParts.Intersect(staticParts));
        Assert.Empty(inputParts.Intersect(staticParts));
    }
}
