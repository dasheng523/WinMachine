using LanguageExt;
using MachineOrchestration.Automation.Types;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Automation.Storage;

/// <summary>自动化逻辑管理器实现（纯函数）</summary>
/// <remarks>
/// 使用不可变的 HashMap 存储自动化逻辑。
/// 所有操作都是纯函数，返回新的管理器实例。
/// 线程安全且可预测。
/// 验证：需求 14.1-14.5
/// </remarks>
public sealed class AutomationLogicManager : IAutomationLogicManager
{
    private readonly HashMap<LogicId, AutomationLogic> _logics;
    
    /// <summary>
    /// 创建空的自动化逻辑管理器
    /// </summary>
    public AutomationLogicManager() : this(HashMap<LogicId, AutomationLogic>())
    {
    }
    
    /// <summary>
    /// 使用指定的逻辑映射创建管理器
    /// </summary>
    /// <param name="logics">逻辑映射</param>
    private AutomationLogicManager(HashMap<LogicId, AutomationLogic> logics)
    {
        _logics = logics;
    }
    
    /// <summary>
    /// 添加自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数实现：
    /// 1. 验证输入（名称、AST）
    /// 2. 检查 ID 是否已存在
    /// 3. 返回包含新逻辑的新管理器实例
    /// 验证：需求 14.1-14.2
    /// </remarks>
    public Either<LogicError, IAutomationLogicManager> AddLogic(AutomationLogic logic)
    {
        // 验证逻辑对象
        if (logic == null)
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.InvalidAst("AutomationLogic cannot be null"));
        }
        
        // 验证名称
        if (string.IsNullOrWhiteSpace(logic.Name))
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.InvalidLogicName(logic.Name ?? ""));
        }
        
        // 验证 AST
        if (logic.Ast == null)
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.InvalidAst());
        }
        
        // 检查 ID 是否已存在
        if (_logics.ContainsKey(logic.Id))
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.LogicAlreadyExists(logic.Id));
        }
        
        // 添加逻辑并返回新的管理器实例
        var newLogics = _logics.Add(logic.Id, logic);
        return Right<LogicError, IAutomationLogicManager>(
            new AutomationLogicManager(newLogics));
    }
    
    /// <summary>
    /// 获取指定 ID 的自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数实现：直接从不可变映射中查找。
    /// 验证：需求 14.1, 14.3
    /// </remarks>
    public Option<AutomationLogic> GetLogic(LogicId id)
    {
        return _logics.Find(id);
    }
    
    /// <summary>
    /// 列出所有逻辑 ID
    /// </summary>
    /// <remarks>
    /// 纯函数实现：返回所有键的序列。
    /// 验证：需求 14.1, 14.3
    /// </remarks>
    public Seq<LogicId> ListLogics()
    {
        return _logics.Keys.ToSeq();
    }
    
    /// <summary>
    /// 移除指定 ID 的自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数实现：
    /// 1. 检查逻辑是否存在
    /// 2. 返回移除逻辑后的新管理器实例
    /// </remarks>
    public Either<LogicError, IAutomationLogicManager> RemoveLogic(LogicId id)
    {
        // 检查逻辑是否存在
        if (!_logics.ContainsKey(id))
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.LogicNotFound(id));
        }
        
        // 移除逻辑并返回新的管理器实例
        var newLogics = _logics.Remove(id);
        return Right<LogicError, IAutomationLogicManager>(
            new AutomationLogicManager(newLogics));
    }
    
    /// <summary>
    /// 更新指定 ID 的自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数实现：
    /// 1. 验证输入
    /// 2. 检查逻辑是否存在
    /// 3. 返回更新逻辑后的新管理器实例
    /// </remarks>
    public Either<LogicError, IAutomationLogicManager> UpdateLogic(AutomationLogic logic)
    {
        // 验证逻辑对象
        if (logic == null)
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.InvalidAst("AutomationLogic cannot be null"));
        }
        
        // 验证名称
        if (string.IsNullOrWhiteSpace(logic.Name))
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.InvalidLogicName(logic.Name ?? ""));
        }
        
        // 验证 AST
        if (logic.Ast == null)
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.InvalidAst());
        }
        
        // 检查逻辑是否存在
        if (!_logics.ContainsKey(logic.Id))
        {
            return Left<LogicError, IAutomationLogicManager>(
                new LogicError.LogicNotFound(logic.Id));
        }
        
        // 更新逻辑并返回新的管理器实例
        var newLogics = _logics.SetItem(logic.Id, logic);
        return Right<LogicError, IAutomationLogicManager>(
            new AutomationLogicManager(newLogics));
    }
    
    /// <summary>
    /// 获取所有自动化逻辑
    /// </summary>
    /// <remarks>
    /// 辅助方法：返回所有逻辑的序列。
    /// </remarks>
    public Seq<AutomationLogic> GetAllLogics()
    {
        return _logics.Values.ToSeq();
    }
    
    /// <summary>
    /// 获取逻辑数量
    /// </summary>
    public int Count => _logics.Count;
    
    /// <summary>
    /// 检查是否为空
    /// </summary>
    public bool IsEmpty => _logics.IsEmpty;
}
