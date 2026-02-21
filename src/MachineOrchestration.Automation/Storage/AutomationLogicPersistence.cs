using System;
using System.IO;
using System.Linq;
using LanguageExt;
using MachineOrchestration.Automation.Types;
using MachineOrchestration.Configuration.Persistence;
using MachineOrchestration.Configuration.Serialization;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Automation.Storage;

/// <summary>自动化逻辑持久化实现（副作用边界）</summary>
/// <remarks>
/// 使用异步文件 I/O 实现自动化逻辑的保存和加载。
/// 委托给 IAutomationLogicSerializer 进行序列化/反序列化。
/// 优雅地处理文件系统错误。
/// 验证：需求 14.4-14.5, 23.1-23.5
/// </remarks>
public sealed class AutomationLogicPersistence : IAutomationLogicPersistence
{
    private readonly IAutomationLogicSerializer _serializer;
    
    public AutomationLogicPersistence(IAutomationLogicSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }
    
    /// <summary>
    /// 保存自动化逻辑到文件
    /// </summary>
    /// <remarks>
    /// 副作用：写入文件系统
    /// 验证：需求 14.4-14.5, 23.1, 23.3
    /// </remarks>
    public async Task<Either<IoError, Unit>> Save(AutomationLogic logic, string filePath)
    {
        if (logic == null)
            throw new ArgumentNullException(nameof(logic));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        
        try
        {
            // 序列化逻辑（纯函数）
            var serializeResult = _serializer.Serialize(logic);
            
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
    /// 从文件加载自动化逻辑
    /// </summary>
    /// <remarks>
    /// 副作用：读取文件系统
    /// 验证：需求 14.4-14.5, 23.2, 23.5
    /// </remarks>
    public async Task<Either<IoError, AutomationLogic>> Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        
        try
        {
            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                return Left<IoError, AutomationLogic>(new IoError.FileNotFound(filePath));
            }
            
            // 读取文件（副作用）
            var json = await File.ReadAllTextAsync(filePath);
            
            // 反序列化逻辑（纯函数）
            var deserializeResult = _serializer.Deserialize(json);
            
            // 处理反序列化错误
            if (deserializeResult.IsLeft)
            {
                var error = deserializeResult.LeftAsEnumerable().First();
                return Left<IoError, AutomationLogic>(new IoError.IoOperationFailed(
                    $"Deserialization failed: {GetDeserializationErrorMessage(error)}",
                    error is DeserializationError.JsonDeserializationFailed jsonError 
                        ? jsonError.InnerException 
                        : null));
            }
            
            var logic = deserializeResult.RightAsEnumerable().First();
            return Right<IoError, AutomationLogic>(logic);
        }
        catch (FileNotFoundException)
        {
            return Left<IoError, AutomationLogic>(new IoError.FileNotFound(filePath));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Left<IoError, AutomationLogic>(new IoError.AccessDenied(
                filePath, 
                $"Access denied: {ex.Message}"));
        }
        catch (PathTooLongException)
        {
            return Left<IoError, AutomationLogic>(new IoError.PathTooLong(filePath));
        }
        catch (DirectoryNotFoundException)
        {
            return Left<IoError, AutomationLogic>(new IoError.DirectoryNotFound(
                Path.GetDirectoryName(filePath) ?? filePath));
        }
        catch (IOException ex) when (IsFileLocked(ex))
        {
            return Left<IoError, AutomationLogic>(new IoError.FileInUse(filePath));
        }
        catch (IOException ex)
        {
            return Left<IoError, AutomationLogic>(new IoError.IoOperationFailed(
                $"I/O operation failed: {ex.Message}", 
                ex));
        }
        catch (Exception ex)
        {
            return Left<IoError, AutomationLogic>(new IoError.IoOperationFailed(
                $"Unexpected error: {ex.Message}", 
                ex));
        }
    }
    
    /// <summary>
    /// 保存所有自动化逻辑到目录
    /// </summary>
    public async Task<Either<IoError, Unit>> SaveAll(IAutomationLogicManager manager, string directoryPath)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));
        
        try
        {
            // 确保目录存在
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            // 获取所有逻辑
            var logics = (manager as AutomationLogicManager)?.GetAllLogics() ?? Seq<AutomationLogic>();
            
            // 保存每个逻辑
            foreach (var logic in logics)
            {
                var fileName = $"{logic.Id}.json";
                var filePath = Path.Combine(directoryPath, fileName);
                
                var saveResult = await Save(logic, filePath);
                if (saveResult.IsLeft)
                {
                    return saveResult;
                }
            }
            
            return Right<IoError, Unit>(unit);
        }
        catch (Exception ex)
        {
            return Left<IoError, Unit>(new IoError.IoOperationFailed(
                $"Failed to save all logics: {ex.Message}", 
                ex));
        }
    }
    
    /// <summary>
    /// 从目录加载所有自动化逻辑
    /// </summary>
    public async Task<Either<IoError, IAutomationLogicManager>> LoadAll(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Directory path cannot be null or empty", nameof(directoryPath));
        
        try
        {
            // 检查目录是否存在
            if (!Directory.Exists(directoryPath))
            {
                return Left<IoError, IAutomationLogicManager>(
                    new IoError.DirectoryNotFound(directoryPath));
            }
            
            // 获取所有 JSON 文件
            var files = Directory.GetFiles(directoryPath, "*.json");
            
            // 创建空管理器
            IAutomationLogicManager manager = new AutomationLogicManager();
            
            // 加载每个文件
            foreach (var file in files)
            {
                var loadResult = await Load(file);
                
                if (loadResult.IsLeft)
                {
                    // 跳过无法加载的文件，继续处理其他文件
                    continue;
                }
                
                var logic = loadResult.RightAsEnumerable().First();
                var addResult = manager.AddLogic(logic);
                
                if (addResult.IsRight)
                {
                    manager = addResult.RightAsEnumerable().First();
                }
            }
            
            return Right<IoError, IAutomationLogicManager>(manager);
        }
        catch (Exception ex)
        {
            return Left<IoError, IAutomationLogicManager>(new IoError.IoOperationFailed(
                $"Failed to load all logics: {ex.Message}", 
                ex));
        }
    }
    
    // Helper methods
    private static bool IsFileLocked(IOException ex)
    {
        const int ERROR_SHARING_VIOLATION = 32;
        const int ERROR_LOCK_VIOLATION = 33;
        
        var errorCode = ex.HResult & 0xFFFF;
        return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
    }
    
    private static bool IsDiskFull(IOException ex)
    {
        const int ERROR_DISK_FULL = 112;
        const int ERROR_HANDLE_DISK_FULL = 39;
        
        var errorCode = ex.HResult & 0xFFFF;
        return errorCode == ERROR_DISK_FULL || errorCode == ERROR_HANDLE_DISK_FULL;
    }
    
    private static string GetSerializationErrorMessage(SerializationError error) =>
        error switch
        {
            SerializationError.JsonSerializationFailed e => e.Message,
            SerializationError.InvalidDataStructure e => e.Message,
            _ => "Unknown serialization error"
        };
    
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
