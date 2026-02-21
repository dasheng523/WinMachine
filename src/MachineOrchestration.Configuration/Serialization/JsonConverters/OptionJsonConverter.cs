using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;

namespace MachineOrchestration.Configuration.Serialization;

/// <summary>Option&lt;T&gt; JSON 转换器工厂</summary>
/// <remarks>
/// 处理 LanguageExt Option&lt;T&gt; 类型的序列化和反序列化。
/// Some(value) 序列化为 value，None 序列化为 null。
/// 验证：需求 23.1-23.3
/// </remarks>
public sealed class OptionJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        
        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(Option<>);
    }
    
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

/// <summary>Option&lt;T&gt; JSON 转换器</summary>
internal sealed class OptionJsonConverter<T> : JsonConverter<Option<T>>
{
    public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Option<T>.None;
        }
        
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return value != null ? Option<T>.Some(value) : Option<T>.None;
    }
    
    public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
    {
        value.Match(
            Some: v => JsonSerializer.Serialize(writer, v, options),
            None: () => writer.WriteNullValue()
        );
    }
}
