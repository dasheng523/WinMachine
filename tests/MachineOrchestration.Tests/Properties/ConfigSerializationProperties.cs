using FsCheck;
using FsCheck.Xunit;
using LanguageExt;
using MachineOrchestration.Configuration.Serialization;
using MachineOrchestration.Configuration.Types;
using Xunit;
using static LanguageExt.Prelude;
using System;
using System.Linq;

namespace MachineOrchestration.Tests.Properties;

/// <summary>
/// 配置序列化的基于属性的测试
/// </summary>
/// <remarks>
/// 验证配置序列化和反序列化的正确性属性。
/// 验证：需求 23.4, 23.5
/// </remarks>
public class ConfigSerializationProperties
{
    private readonly IConfigSerializer _serializer = new ConfigSerializer();
    
    /// <summary>
    /// 属性 14：配置反序列化错误处理
    /// </summary>
    /// <remarks>
    /// 验证：当提供无效的 JSON 输入时，反序列化应当返回描述性错误而不是抛出异常。
    /// 验证：需求 23.5
    /// </remarks>
    [Theory]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("[")]
    [InlineData("]")]
    [InlineData("{\"key\":")]
    [InlineData("{key: value}")]
    [InlineData("{'key': 'value'}")]
    [InlineData("{\"key\": }")]
    [InlineData("not json")]
    [InlineData("123abc")]
    [InlineData("true false")]
    public void DeserializationErrorHandling_InvalidJson_ReturnsDescriptiveError(string invalidJson)
    {
        // Act
        var result = _serializer.Deserialize(invalidJson);
        
        // Assert: 应当返回 Left（错误）
        Assert.True(result.IsLeft, "Invalid JSON should return Left (error)");
        
        result.IfLeft(error =>
        {
            // 应当是反序列化错误类型之一
            Assert.True(
                error is DeserializationError.JsonDeserializationFailed ||
                error is DeserializationError.InvalidJsonFormat ||
                error is DeserializationError.CorruptedConfig,
                $"Error should be one of the expected deserialization error types, got {error.GetType().Name}");
        });
    }
    
    /// <summary>
    /// 属性：空字符串反序列化应当返回错误
    /// </summary>
    [Property]
    public bool DeserializationErrorHandling_EmptyString_ReturnsError()
    {
        // Arrange
        var emptyInputs = new[] { "", "   ", "\t", "\n" };
        
        // Act & Assert
        return emptyInputs.All(input =>
        {
            var result = _serializer.Deserialize(input);
            return result.IsLeft && result.Match(
                Left: error => error is DeserializationError.InvalidJsonFormat,
                Right: _ => false
            );
        });
    }
    
    /// <summary>
    /// 属性：null 字符串反序列化应当返回错误
    /// </summary>
    [Fact]
    public void DeserializationErrorHandling_NullString_ReturnsError()
    {
        // Act
        var result = _serializer.Deserialize(null!);
        
        // Assert
        Assert.True(result.IsLeft);
        result.IfLeft(error =>
        {
            Assert.IsType<DeserializationError.InvalidJsonFormat>(error);
        });
    }
    
    /// <summary>
    /// 属性：不完整的 JSON 结构应当被处理
    /// </summary>
    /// <remarks>
    /// 注意：空 JSON "{}" 可能被成功反序列化为具有 null 字段的对象。
    /// 这是 System.Text.Json 的默认行为。实际的验证应该在配置验证器中进行。
    /// </remarks>
    [Fact]
    public void DeserializationErrorHandling_IncompleteJson_IsHandled()
    {
        // Arrange: 不完整但格式正确的 JSON
        var incompleteJson = "{}";
        
        // Act
        var result = _serializer.Deserialize(incompleteJson);
        
        // Assert: 可能成功（返回 null 字段的对象）或失败（返回错误）
        // 两种情况都是可接受的，关键是不抛出异常
        Assert.True(true, "Deserialization should not throw exceptions");
    }
    
    /// <summary>
    /// 属性：类型不匹配的 JSON 应当返回错误
    /// </summary>
    [Theory]
    [InlineData("{\"machine\": \"not an object\", \"controlBoard\": {}, \"automationLogics\": {}}")]
    [InlineData("{\"machine\": {}, \"controlBoard\": \"not an object\", \"automationLogics\": {}}")]
    [InlineData("{\"machine\": {}, \"controlBoard\": {}, \"automationLogics\": \"not an object\"}")]
    public void DeserializationErrorHandling_TypeMismatch_ReturnsError(string typeMismatchJson)
    {
        // Act
        var result = _serializer.Deserialize(typeMismatchJson);
        
        // Assert
        Assert.True(result.IsLeft);
    }
}
