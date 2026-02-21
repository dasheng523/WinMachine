using LanguageExt;
using MachineOrchestration.Configuration.Serialization;
using MachineOrchestration.Configuration.Types;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Configuration.Persistence;

/// <summary>配置持久化实现（副作用边界）</summary>
/// <remarks>
/// 使用异步文件 I/O 实现配置的保存和加载。
/// 委托给 IConfigSerializer 进行序列化/反序列化。
/// 优雅地处理文件系统错误。
/// 验证：需求 23.1-23.5
/// </remarks>
public sealed class ConfigPersistence : IConfigPersistence
{
    private readonly IConfigSerializer _serializer;
    
    public ConfigPersistence(IConfigSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }
    
    /// <summary>
    /// 保存机器配置到文件
    /// </summary>
    /// <remarks>
    /// 副作用：写入文件系统
    /// 验证：需求 23.1, 23.3
    /// </remarks>
    public async Task<Either<IoError, Unit>> Save(MachineConfig config, string filePath)
    {
        // Validate parameters (throw exceptions for programming errors)
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        
        try
        {
            // 序列化配置（纯函数）
            var serializeResult = _serializer.Serialize(config);
            
            // 处理序列化错误
            if (serializeResult.IsLeft)
            {
                var error = serializeResult.LeftAsEnumerable().First();
                return Left<IoError, Unit>(new IoError.IoOperationFailed(
                    $"Serialization failed: {GetSerializationErrorMessage(error)}",
                    error is SerializationError.JsonSerializationFailed jsonError 
                        ? jsonError.InnerException 
                        : null));
            }
            
            var json = serializeResult.RightAsEnumerable().First();
            
            // 确保目录存在
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Left<IoError, Unit>(new IoError.AccessDenied(
                        directory, 
                        $"Cannot create directory: {ex.Message}"));
                }
                catch (IOException ex)
                {
                    return Left<IoError, Unit>(new IoError.IoOperationFailed(
                        $"Failed to create directory: {ex.Message}", 
                        ex));
                }
            }
            
            // 写入文件（副作用）
            await File.WriteAllTextAsync(filePath, json);
            
            return Right<IoError, Unit>(unit);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Left<IoError, Unit>(new IoError.AccessDenied(
                filePath, 
                $"Access denied: {ex.Message}"));
        }
        catch (PathTooLongException)
        {
            return Left<IoError, Unit>(new IoError.PathTooLong(filePath));
        }
        catch (DirectoryNotFoundException)
        {
            return Left<IoError, Unit>(new IoError.DirectoryNotFound(
                Path.GetDirectoryName(filePath) ?? filePath));
        }
        catch (IOException ex) when (IsFileLocked(ex))
        {
            return Left<IoError, Unit>(new IoError.FileInUse(filePath));
        }
        catch (IOException ex) when (IsDiskFull(ex))
        {
            return Left<IoError, Unit>(new IoError.DiskFull(ex.Message));
        }
        catch (IOException ex)
        {
            return Left<IoError, Unit>(new IoError.IoOperationFailed(
                $"I/O operation failed: {ex.Message}", 
                ex));
        }
        catch (Exception ex)
        {
            return Left<IoError, Unit>(new IoError.IoOperationFailed(
                $"Unexpected error: {ex.Message}", 
                ex));
        }
    }
    
    /// <summary>
    /// 从文件加载机器配置
    /// </summary>
    /// <remarks>
    /// 副作用：读取文件系统
    /// 验证：需求 23.2, 23.5
    /// </remarks>
    public async Task<Either<IoError, MachineConfig>> Load(string filePath)
    {
        // Validate parameters (throw exceptions for programming errors)
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        
        try
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                return Left<IoError, MachineConfig>(new IoError.FileNotFound(filePath));
            }
            
            // 读取文件（副作用）
            var json = await File.ReadAllTextAsync(filePath);
            
            // 反序列化配置（纯函数）
            var deserializeResult = _serializer.Deserialize(json);
            
            // 处理反序列化错误
            if (deserializeResult.IsLeft)
            {
                var error = deserializeResult.LeftAsEnumerable().First();
                return Left<IoError, MachineConfig>(new IoError.IoOperationFailed(
                    $"Deserialization failed: {GetDeserializationErrorMessage(error)}",
                    error is DeserializationError.JsonDeserializationFailed jsonError 
                        ? jsonError.InnerException 
                        : null));
            }
            
            var config = deserializeResult.RightAsEnumerable().First();
            return Right<IoError, MachineConfig>(config);
        }
        catch (FileNotFoundException)
        {
            return Left<IoError, MachineConfig>(new IoError.FileNotFound(filePath));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Left<IoError, MachineConfig>(new IoError.AccessDenied(
                filePath, 
                $"Access denied: {ex.Message}"));
        }
        catch (PathTooLongException)
        {
            return Left<IoError, MachineConfig>(new IoError.PathTooLong(filePath));
        }
        catch (DirectoryNotFoundException)
        {
            return Left<IoError, MachineConfig>(new IoError.DirectoryNotFound(
                Path.GetDirectoryName(filePath) ?? filePath));
        }
        catch (IOException ex) when (IsFileLocked(ex))
        {
            return Left<IoError, MachineConfig>(new IoError.FileInUse(filePath));
        }
        catch (IOException ex)
        {
            return Left<IoError, MachineConfig>(new IoError.IoOperationFailed(
                $"I/O operation failed: {ex.Message}", 
                ex));
        }
        catch (Exception ex)
        {
            return Left<IoError, MachineConfig>(new IoError.IoOperationFailed(
                $"Unexpected error: {ex.Message}", 
                ex));
        }
    }
    
    /// <summary>检查是否为文件被锁定错误</summary>
    private static bool IsFileLocked(IOException ex)
    {
        const int ERROR_SHARING_VIOLATION = 32;
        const int ERROR_LOCK_VIOLATION = 33;
        
        var errorCode = ex.HResult & 0xFFFF;
        return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
    }
    
    /// <summary>检查是否为磁盘空间不足错误</summary>
    private static bool IsDiskFull(IOException ex)
    {
        const int ERROR_DISK_FULL = 112;
        const int ERROR_HANDLE_DISK_FULL = 39;
        
        var errorCode = ex.HResult & 0xFFFF;
        return errorCode == ERROR_DISK_FULL || errorCode == ERROR_HANDLE_DISK_FULL;
    }
    
    /// <summary>获取序列化错误消息</summary>
    private static string GetSerializationErrorMessage(SerializationError error) =>
        error switch
        {
            SerializationError.JsonSerializationFailed e => e.Message,
            SerializationError.InvalidDataStructure e => e.Message,
            _ => "Unknown serialization error"
        };
    
    /// <summary>获取反序列化错误消息</summary>
    private static string GetDeserializationErrorMessage(DeserializationError error) =>
        error switch
        {
            DeserializationError.JsonDeserializationFailed e => e.Message,
            DeserializationError.InvalidJsonFormat e => e.Message,
            DeserializationError.MissingRequiredField e => $"Missing required field: {e.FieldName}",
            DeserializationError.TypeMismatch e => $"Type mismatch: expected {e.ExpectedType}, got {e.ActualType}",
            DeserializationError.CorruptedConfig e => $"Corrupted config: {e.Message}",
            _ => "Unknown deserialization error"
        };
}
