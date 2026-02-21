using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Configuration.Serialization;

/// <summary>Seq&lt;T&gt; JSON 转换器工厂</summary>
/// <remarks>
/// 处理 LanguageExt Seq&lt;T&gt; 类型的序列化和反序列化。
/// Seq&lt;T&gt; 序列化为 JSON 数组。
/// 验证：需求 23.1-23.3
/// </remarks>
public sealed class SeqJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        
        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(Seq<>);
    }
    
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(SeqJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

/// <summary>Seq&lt;T&gt; JSON 转换器</summary>
internal sealed class SeqJsonConverter<T> : JsonConverter<Seq<T>>
{
    public override Seq<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return Seq<T>();
        }
        
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected StartArray token, got {reader.TokenType}");
        }
        
        var list = new List<T>();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }
            
            var item = JsonSerializer.Deserialize<T>(ref reader, options);
            if (item != null)
            {
                list.Add(item);
            }
        }
        
        return list.ToSeq();
    }
    
    public override void Write(Utf8JsonWriter writer, Seq<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        
        foreach (var item in value)
        {
            JsonSerializer.Serialize(writer, item, options);
        }
        
        writer.WriteEndArray();
    }
}
