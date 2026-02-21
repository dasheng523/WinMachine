using LanguageExt;
using MachineOrchestration.Automation.Types;
using MachineOrchestration.Configuration.Persistence;

namespace MachineOrchestration.Automation.Storage;

/// <summary>自动化逻辑持久化接口（副作用边界）</summary>
/// <remarks>
/// 负责自动化逻辑的文件系统持久化操作。
/// 所有方法都包含副作用（文件 I/O）。
/// 验证：需求 14.4-14.5, 23.1-23.5
/// </remarks>
public interface IAutomationLogicPersistence
{
    /// <summary>
    /// 保存自动化逻辑到文件
    /// </summary>
    /// <remarks>
    /// 副作用：写入文件系统
    /// 验证：需求 14.4-14.5, 23.1, 23.3
    /// </remarks>
    /// <param name="logic">要保存的自动化逻辑</param>
    /// <param name="filePath">文件路径</param>
    /// <returns>
    /// Right: Unit（成功）
    /// Left: I/O 错误
    /// </returns>
    Task<Either<IoError, Unit>> Save(AutomationLogic logic, string filePath);
    
    /// <summary>
    /// 从文件加载自动化逻辑
    /// </summary>
    /// <remarks>
    /// 副作用：读取文件系统
    /// 验证：需求 14.4-14.5, 23.2, 23.5
    /// </remarks>
    /// <param name="filePath">文件路径</param>
    /// <returns>
    /// Right: 自动化逻辑
    /// Left: I/O 错误
    /// </returns>
    Task<Either<IoError, AutomationLogic>> Load(string filePath);
    
    /// <summary>
    /// 保存所有自动化逻辑到目录
    /// </summary>
    /// <remarks>
    /// 副作用：写入文件系统
    /// 每个逻辑保存为单独的文件，文件名为逻辑 ID。
    /// </remarks>
    /// <param name="manager">自动化逻辑管理器</param>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>
    /// Right: Unit（成功）
    /// Left: I/O 错误
    /// </returns>
    Task<Either<IoError, Unit>> SaveAll(IAutomationLogicManager manager, string directoryPath);
    
    /// <summary>
    /// 从目录加载所有自动化逻辑
    /// </summary>
    /// <remarks>
    /// 副作用：读取文件系统
    /// 加载目录中所有 .json 文件。
    /// </remarks>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>
    /// Right: 包含所有逻辑的管理器
    /// Left: I/O 错误
    /// </returns>
    Task<Either<IoError, IAutomationLogicManager>> LoadAll(string directoryPath);
}
