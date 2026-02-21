using LanguageExt;
using MachineOrchestration.Automation.Types;

namespace MachineOrchestration.Automation.Storage;

/// <summary>自动化逻辑管理器接口（纯函数）</summary>
/// <remarks>
/// 管理自动化逻辑的添加、检索和列表操作。
/// 所有操作都是纯函数，返回新的管理器实例而不是修改现有实例。
/// 使用不可变数据结构确保线程安全和可预测性。
/// 验证：需求 14.1-14.3
/// </remarks>
public interface IAutomationLogicManager
{
    /// <summary>
    /// 添加自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数：返回新的管理器实例，不修改当前实例。
    /// 如果逻辑 ID 已存在，返回 LogicAlreadyExists 错误。
    /// 如果逻辑名称无效，返回 InvalidLogicName 错误。
    /// 如果 AST 为 null，返回 InvalidAst 错误。
    /// 验证：需求 14.1-14.2
    /// </remarks>
    /// <param name="logic">要添加的自动化逻辑</param>
    /// <returns>
    /// Right: 包含新逻辑的新管理器实例
    /// Left: 逻辑错误（LogicAlreadyExists, InvalidLogicName, InvalidAst）
    /// </returns>
    Either<LogicError, IAutomationLogicManager> AddLogic(AutomationLogic logic);
    
    /// <summary>
    /// 获取指定 ID 的自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数：不修改管理器状态。
    /// 如果逻辑存在，返回 Some(logic)；否则返回 None。
    /// 验证：需求 14.1, 14.3
    /// </remarks>
    /// <param name="id">逻辑 ID</param>
    /// <returns>
    /// Some: 找到的自动化逻辑
    /// None: 逻辑不存在
    /// </returns>
    Option<AutomationLogic> GetLogic(LogicId id);
    
    /// <summary>
    /// 列出所有逻辑 ID
    /// </summary>
    /// <remarks>
    /// 纯函数：返回所有已存储逻辑的 ID 序列。
    /// 验证：需求 14.1, 14.3
    /// </remarks>
    /// <returns>所有逻辑 ID 的不可变序列</returns>
    Seq<LogicId> ListLogics();
    
    /// <summary>
    /// 移除指定 ID 的自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数：返回新的管理器实例，不修改当前实例。
    /// 如果逻辑不存在，返回 LogicNotFound 错误。
    /// </remarks>
    /// <param name="id">要移除的逻辑 ID</param>
    /// <returns>
    /// Right: 移除逻辑后的新管理器实例
    /// Left: LogicNotFound 错误
    /// </returns>
    Either<LogicError, IAutomationLogicManager> RemoveLogic(LogicId id);
    
    /// <summary>
    /// 更新指定 ID 的自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数：返回新的管理器实例，不修改当前实例。
    /// 如果逻辑不存在，返回 LogicNotFound 错误。
    /// </remarks>
    /// <param name="logic">更新后的自动化逻辑</param>
    /// <returns>
    /// Right: 更新逻辑后的新管理器实例
    /// Left: LogicNotFound 或其他逻辑错误
    /// </returns>
    Either<LogicError, IAutomationLogicManager> UpdateLogic(AutomationLogic logic);
}
