using LanguageExt;
using MachineOrchestration.Configuration.Types;

namespace MachineOrchestration.Configuration.Persistence;

/// <summary>配置持久化接口（副作用边界）</summary>
/// <remarks>
/// 提供配置的文件系统持久化功能。
/// 所有方法都包含副作用（文件 I/O）。
/// 验证：需求 23.1-23.2
/// </remarks>
public interface IConfigPersistence
{
    /// <summary>
    /// 保存机器配置到文件
    /// </summary>
    /// <param name="config">要保存的机器配置</param>
    /// <param name="filePath">文件路径</param>
    /// <returns>成功时返回 Unit，失败时返回 I/O 错误</returns>
    /// <remarks>
    /// 副作用：写入文件系统
    /// 验证：需求 23.1
    /// </remarks>
    Task<Either<IoError, Unit>> Save(MachineConfig config, string filePath);
    
    /// <summary>
    /// 从文件加载机器配置
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>成功时返回机器配置，失败时返回 I/O 错误</returns>
    /// <remarks>
    /// 副作用：读取文件系统
    /// 验证：需求 23.2
    /// </remarks>
    Task<Either<IoError, MachineConfig>> Load(string filePath);
}
