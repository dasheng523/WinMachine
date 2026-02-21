using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using MachineOrchestration.Automation.Types;
using MachineOrchestration.Configuration.Serialization;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Automation.Storage;

/// <summary>自动化逻辑序列化器实现</summary>
/// <remarks>
/// 使用 System.Text.Json 实现自动化逻辑的序列化和反序列化。
/// 支持 AST 的多态类型序列化。
/// 验证：需求 14.4-14.5, 23.1-23.3
/// </remarks>
public sealed class AutomationLogicSerializer : IAutomationLogicSerializer
{
    private readonly JsonSerializerOptions _options;
    
    /// <summary>创建自动化逻辑序列化器实例</summary>
    public AutomationLogicSerializer()
    {
        _options = CreateJsonOptions();
    }
    
    /// <summary>
    /// 将自动化逻辑序列化为 JSON 字符串
    /// </summary>
    /// <remarks>
    /// 纯函数实现：捕获所有异常并转换为 Either 类型。
    /// 验证：需求 14.4-14.5, 23.1
    /// </remarks>
    public Either<SerializationError, string> Serialize(AutomationLogic logic)
    {
        try
        {
            if (logic == null)
            {
                return Left<SerializationError, string>(
                    new SerializationError.InvalidDataStructure("AutomationLogic cannot be null"));
            }
            
            var json = JsonSerializer.Serialize(logic, _options);
            return Right<SerializationError, string>(json);
        }
        catch (JsonException ex)
        {
            return Left<SerializationError, string>(
                new SerializationError.JsonSerializationFailed(
                    $"JSON serialization failed: {ex.Message}", ex));
        }
        catch (Exception ex)
        {
            return Left<SerializationError, string>(
                new SerializationError.JsonSerializationFailed(
                    $"Unexpected error during serialization: {ex.Message}", ex));
        }
    }
    
    /// <summary>
    /// 从 JSON 字符串反序列化自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数实现：捕获所有异常并转换为 Either 类型。
    /// 验证：需求 14.4-14.5, 23.2, 23.5
    /// </remarks>
    public Either<DeserializationError, AutomationLogic> Deserialize(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Left<DeserializationError, AutomationLogic>(
                    new DeserializationError.InvalidJsonFormat("JSON string cannot be null or empty"));
            }
            
            var logic = JsonSerializer.Deserialize<AutomationLogic>(json, _options);
            
            if (logic == null)
            {
                return Left<DeserializationError, AutomationLogic>(
                    new DeserializationError.CorruptedConfig("Deserialization resulted in null logic"));
            }
            
            return Right<DeserializationError, AutomationLogic>(logic);
        }
        catch (JsonException ex)
        {
            return Left<DeserializationError, AutomationLogic>(
                new DeserializationError.JsonDeserializationFailed(
                    $"JSON deserialization failed: {ex.Message}", ex));
        }
        catch (Exception ex)
        {
            return Left<DeserializationError, AutomationLogic>(
                new DeserializationError.CorruptedConfig(
                    $"Unexpected error during deserialization: {ex.Message}"));
        }
    }
    
    /// <summary>
    /// 创建 JSON 序列化选项
    /// </summary>
    /// <remarks>
    /// 配置多态类型序列化、Option&lt;T&gt; 处理和代数数据类型支持。
    /// 验证：需求 14.4-14.5, 23.1-23.3
    /// </remarks>
    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IncludeFields = false,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };
        
        // 添加自定义转换器
        options.Converters.Add(new OptionJsonConverterFactory());
        options.Converters.Add(new SeqJsonConverterFactory());
        options.Converters.Add(new JsonStringEnumConverter());
        
        return options;
    }
}
