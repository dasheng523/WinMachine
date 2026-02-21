using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using MachineOrchestration.Configuration.Types;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Configuration.Serialization;

/// <summary>配置序列化器实现</summary>
/// <remarks>
/// 使用 System.Text.Json 实现配置的序列化和反序列化。
/// 支持多态类型、Option&lt;T&gt; 和代数数据类型的序列化。
/// 验证：需求 23.1-23.3
/// </remarks>
public sealed class ConfigSerializer : IConfigSerializer
{
    private readonly JsonSerializerOptions _options;
    
    /// <summary>创建配置序列化器实例</summary>
    public ConfigSerializer()
    {
        _options = CreateJsonOptions();
    }
    
    /// <summary>
    /// 将机器配置序列化为 JSON 字符串
    /// </summary>
    /// <remarks>
    /// 纯函数实现：捕获所有异常并转换为 Either 类型。
    /// 验证：需求 23.1
    /// </remarks>
    public Either<SerializationError, string> Serialize(MachineConfig config)
    {
        try
        {
            if (config == null)
            {
                return Left<SerializationError, string>(
                    new SerializationError.InvalidDataStructure("MachineConfig cannot be null"));
            }
            
            var json = JsonSerializer.Serialize(config, _options);
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
    /// 从 JSON 字符串反序列化机器配置
    /// </summary>
    /// <remarks>
    /// 纯函数实现：捕获所有异常并转换为 Either 类型。
    /// 验证：需求 23.2, 23.5
    /// </remarks>
    public Either<DeserializationError, MachineConfig> Deserialize(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Left<DeserializationError, MachineConfig>(
                    new DeserializationError.InvalidJsonFormat("JSON string cannot be null or empty"));
            }
            
            var config = JsonSerializer.Deserialize<MachineConfig>(json, _options);
            
            if (config == null)
            {
                return Left<DeserializationError, MachineConfig>(
                    new DeserializationError.CorruptedConfig("Deserialization resulted in null config"));
            }
            
            return Right<DeserializationError, MachineConfig>(config);
        }
        catch (JsonException ex)
        {
            return Left<DeserializationError, MachineConfig>(
                new DeserializationError.JsonDeserializationFailed(
                    $"JSON deserialization failed: {ex.Message}", ex));
        }
        catch (Exception ex)
        {
            return Left<DeserializationError, MachineConfig>(
                new DeserializationError.CorruptedConfig(
                    $"Unexpected error during deserialization: {ex.Message}"));
        }
    }
    
    /// <summary>
    /// 创建 JSON 序列化选项
    /// </summary>
    /// <remarks>
    /// 配置多态类型序列化、Option&lt;T&gt; 处理和代数数据类型支持。
    /// 验证：需求 23.1-23.3
    /// </remarks>
    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IncludeFields = false,
            // 支持多态类型序列化
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };
        
        // 添加自定义转换器
        options.Converters.Add(new OptionJsonConverterFactory());
        options.Converters.Add(new SeqJsonConverterFactory());
        options.Converters.Add(new HashMapJsonConverterFactory());
        options.Converters.Add(new JsonStringEnumConverter());
        
        return options;
    }
}
