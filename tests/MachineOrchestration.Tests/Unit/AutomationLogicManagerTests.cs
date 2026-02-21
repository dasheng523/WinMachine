using LanguageExt;
using MachineOrchestration.Automation.Storage;
using MachineOrchestration.Automation.Types;
using MachineOrchestration.Dsl.Ast;
using Xunit;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Tests.Unit;

/// <summary>
/// 自动化逻辑管理器单元测试
/// </summary>
/// <remarks>
/// 测试自动化逻辑的添加、检索、列表和移除操作。
/// 验证：需求 14.1-14.5
/// </remarks>
public class AutomationLogicManagerTests
{
    [Fact]
    public void AddLogic_ValidLogic_ReturnsNewManager()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = AutomationLogic.Create("Test Logic", Ast.Empty);
        
        // Act
        var result = manager.AddLogic(logic);
        
        // Assert
        Assert.True(result.IsRight);
        var newManager = result.RightAsEnumerable().First();
        Assert.NotNull(newManager);
        Assert.Equal(1, ((AutomationLogicManager)newManager).Count);
    }
    
    [Fact]
    public void AddLogic_DuplicateId_ReturnsError()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = AutomationLogic.Create("Test Logic", Ast.Empty);
        var addResult = manager.AddLogic(logic);
        var newManager = addResult.RightAsEnumerable().First();
        
        // Act - try to add the same logic again
        var result = newManager.AddLogic(logic);
        
        // Assert
        Assert.True(result.IsLeft);
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<LogicError.LogicAlreadyExists>(error);
    }
    
    [Fact]
    public void AddLogic_NullName_ReturnsError()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = new AutomationLogic(LogicId.NewId(), null!, Ast.Empty);
        
        // Act
        var result = manager.AddLogic(logic);
        
        // Assert
        Assert.True(result.IsLeft);
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<LogicError.InvalidLogicName>(error);
    }
    
    [Fact]
    public void AddLogic_EmptyName_ReturnsError()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = new AutomationLogic(LogicId.NewId(), "", Ast.Empty);
        
        // Act
        var result = manager.AddLogic(logic);
        
        // Assert
        Assert.True(result.IsLeft);
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<LogicError.InvalidLogicName>(error);
    }
    
    [Fact]
    public void AddLogic_WhitespaceName_ReturnsError()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = new AutomationLogic(LogicId.NewId(), "   ", Ast.Empty);
        
        // Act
        var result = manager.AddLogic(logic);
        
        // Assert
        Assert.True(result.IsLeft);
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<LogicError.InvalidLogicName>(error);
    }
    
    [Fact]
    public void AddLogic_NullAst_ReturnsError()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = new AutomationLogic(LogicId.NewId(), "Test Logic", null!);
        
        // Act
        var result = manager.AddLogic(logic);
        
        // Assert
        Assert.True(result.IsLeft);
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<LogicError.InvalidAst>(error);
    }
    
    [Fact]
    public void GetLogic_ExistingId_ReturnsLogic()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = AutomationLogic.Create("Test Logic", Ast.Empty);
        var addResult = manager.AddLogic(logic);
        var newManager = addResult.RightAsEnumerable().First();
        
        // Act
        var result = newManager.GetLogic(logic.Id);
        
        // Assert
        Assert.True(result.IsSome);
        var retrievedLogic = result.IfNone(() => throw new Exception("Logic not found"));
        Assert.Equal(logic.Id, retrievedLogic.Id);
        Assert.Equal(logic.Name, retrievedLogic.Name);
    }
    
    [Fact]
    public void GetLogic_NonExistingId_ReturnsNone()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var nonExistingId = LogicId.NewId();
        
        // Act
        var result = manager.GetLogic(nonExistingId);
        
        // Assert
        Assert.True(result.IsNone);
    }
    
    [Fact]
    public void ListLogics_EmptyManager_ReturnsEmptySequence()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        
        // Act
        var result = manager.ListLogics();
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void ListLogics_WithLogics_ReturnsAllIds()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic1 = AutomationLogic.Create("Logic 1", Ast.Empty);
        var logic2 = AutomationLogic.Create("Logic 2", Ast.Empty);
        var logic3 = AutomationLogic.Create("Logic 3", Ast.Empty);
        
        var result1 = manager.AddLogic(logic1);
        var manager1 = result1.RightAsEnumerable().First();
        
        var result2 = manager1.AddLogic(logic2);
        var manager2 = result2.RightAsEnumerable().First();
        
        var result3 = manager2.AddLogic(logic3);
        var manager3 = result3.RightAsEnumerable().First();
        
        // Act
        var ids = manager3.ListLogics();
        
        // Assert
        Assert.Equal(3, ids.Count);
        Assert.Contains(logic1.Id, ids);
        Assert.Contains(logic2.Id, ids);
        Assert.Contains(logic3.Id, ids);
    }
    
    [Fact]
    public void RemoveLogic_ExistingId_ReturnsNewManager()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = AutomationLogic.Create("Test Logic", Ast.Empty);
        var addResult = manager.AddLogic(logic);
        var newManager = addResult.RightAsEnumerable().First();
        
        // Act
        var result = newManager.RemoveLogic(logic.Id);
        
        // Assert
        Assert.True(result.IsRight);
        var managerAfterRemove = result.RightAsEnumerable().First();
        Assert.Equal(0, ((AutomationLogicManager)managerAfterRemove).Count);
    }
    
    [Fact]
    public void RemoveLogic_NonExistingId_ReturnsError()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var nonExistingId = LogicId.NewId();
        
        // Act
        var result = manager.RemoveLogic(nonExistingId);
        
        // Assert
        Assert.True(result.IsLeft);
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<LogicError.LogicNotFound>(error);
    }
    
    [Fact]
    public void UpdateLogic_ExistingId_ReturnsNewManager()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = AutomationLogic.Create("Original Name", Ast.Empty);
        var addResult = manager.AddLogic(logic);
        var newManager = addResult.RightAsEnumerable().First();
        
        var updatedLogic = logic.WithName("Updated Name");
        
        // Act
        var result = newManager.UpdateLogic(updatedLogic);
        
        // Assert
        Assert.True(result.IsRight);
        var managerAfterUpdate = result.RightAsEnumerable().First();
        var retrievedLogic = managerAfterUpdate.GetLogic(logic.Id);
        Assert.True(retrievedLogic.IsSome);
        Assert.Equal("Updated Name", retrievedLogic.IfNone(() => throw new Exception()).Name);
    }
    
    [Fact]
    public void UpdateLogic_NonExistingId_ReturnsError()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = AutomationLogic.Create("Test Logic", Ast.Empty);
        
        // Act
        var result = manager.UpdateLogic(logic);
        
        // Assert
        Assert.True(result.IsLeft);
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<LogicError.LogicNotFound>(error);
    }
    
    [Fact]
    public void Manager_IsImmutable_OriginalUnchanged()
    {
        // Arrange
        var manager = new AutomationLogicManager();
        var logic = AutomationLogic.Create("Test Logic", Ast.Empty);
        
        // Act
        var result = manager.AddLogic(logic);
        
        // Assert - original manager unchanged
        Assert.Equal(0, manager.Count);
        Assert.True(manager.IsEmpty);
        
        // New manager has the logic
        var newManager = result.RightAsEnumerable().First();
        Assert.Equal(1, ((AutomationLogicManager)newManager).Count);
    }
}
