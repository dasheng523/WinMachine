namespace MachineOrchestration.Configuration.Persistence;

/// <summary>I/O 错误（和类型）</summary>
/// <remarks>
/// 表示文件系统操作过程中可能发生的错误。
/// 验证：需求 23.1-23.5, 24.1-24.6
/// </remarks>
public abstract record IoError
{
    /// <summary>文件未找到</summary>
    public sealed record FileNotFound(string FilePath) : IoError;
    
    /// <summary>访问被拒绝（权限错误）</summary>
    public sealed record AccessDenied(string FilePath, string Message) : IoError;
    
    /// <summary>磁盘空间不足</summary>
    public sealed record DiskFull(string Message) : IoError;
    
    /// <summary>路径过长</summary>
    public sealed record PathTooLong(string FilePath) : IoError;
    
    /// <summary>I/O 操作失败</summary>
    public sealed record IoOperationFailed(string Message, Exception? InnerException = null) : IoError;
    
    /// <summary>目录未找到</summary>
    public sealed record DirectoryNotFound(string DirectoryPath) : IoError;
    
    /// <summary>文件已被占用</summary>
    public sealed record FileInUse(string FilePath) : IoError;
    
    private IoError() { }
}
