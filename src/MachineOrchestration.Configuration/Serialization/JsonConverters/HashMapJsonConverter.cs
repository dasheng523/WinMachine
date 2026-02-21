using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Configuration.Serialization;

/// <summary>HashMap&lt;K, V&gt; JSON 转换器工厂</summary>
/// <remarks>
/// 处理 LanguageExt HashMap&lt;K, V&gt; 类型的序列化和反序列化。
/// HashMap&lt;K, V&gt; 序列化为 JSON 对象（键必须可转换为字符串）。
/// 验证：需求 23.1-23.3
/// </remarks>
public sealed class HashMapJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;
        
        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(HashMap<,>);
    }
    
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = typeToConvert.GetGenericArguments();
        var keyType = typeArgs[0];
        var valueType = typeArgs[1];
        var converterType = typeof(HashMapJsonConverter<,>).MakeGenericType(keyType, valueType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

/// <summary>HashMap&lt;K, V&gt; JSON 转换器</summary>
internal sealed class HashMapJsonConverter<K, V> : JsonConverter<HashMap<K, V>>
    where K : notnull
{
    public override HashMap<K, V> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return HashMap<K, V>();
        }
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject token, got {reader.TokenType}");
        }
        
        var dict = new Dictionary<K, V>();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
            
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected PropertyName token, got {reader.TokenType}");
            }
            
            var keyString = reader.GetString();
            if (keyString == null)
            {
                throw new JsonException("Property name cannot be null");
            }
            
            // 将字符串键转换为 K 类型
            var key = ConvertKey(keyString);
            
            reader.Read();
            var value = JsonSerializer.Deserialize<V>(ref reader, options);
            
            if (value != null)
            {
                dict[key] = value;
            }
        }
        
        return dict.ToHashMap();
    }
    
    public override void Write(Utf8JsonWriter writer, HashMap<K, V> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        foreach (var (key, val) in value)
        {
            var keyString = ConvertKeyToString(key);
            writer.WritePropertyName(keyString);
            JsonSerializer.Serialize(writer, val, options);
        }
        
        writer.WriteEndObject();
    }
    
    private static K ConvertKey(string keyString)
    {
        var keyType = typeof(K);
        
        // 处理常见的键类型
        if (keyType == typeof(string))
        {
            return (K)(object)keyString;
        }
        else if (keyType == typeof(Guid))
        {
            return (K)(object)Guid.Parse(keyString);
        }
        else if (keyType.IsValueType && keyType.IsGenericType)
        {
            // 处理 newtype 模式（如 LogicId）
            var underlyingType = keyType.GetGenericArguments()[0];
            if (underlyingType == typeof(Guid))
            {
                var guid = Guid.Parse(keyString);
                return (K)Activator.CreateInstance(keyType, guid)!;
            }
        }
        
        throw new JsonException($"Unsupported key type: {keyType.Name}");
    }
    
    private static string ConvertKeyToString(K key)
    {
        if (key is string str)
        {
            return str;
        }
        else if (key is Guid guid)
        {
            return guid.ToString();
        }
        else
        {
            // 处理 newtype 模式（如 LogicId）
            var keyType = typeof(K);
            if (keyType.IsValueType && keyType.IsGenericType)
            {
                var valueProperty = keyType.GetProperty("Value");
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(key);
                    if (value is Guid g)
                    {
                        return g.ToString();
                    }
                }
            }
        }
        
        return key.ToString() ?? throw new JsonException("Key cannot be converted to string");
    }
}
