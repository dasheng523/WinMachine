namespace MachineOrchestration.Configuration.Serialization;

/// <summary>序列化错误（和类型）</summary>
/// <remarks>
/// 表示配置序列化过程中可能发生的错误。
/// 验证：需求 23.1-23.2, 24.1-24.6
/// </remarks>
public abstract record SerializationError
{
    /// <summary>JSON 序列化失败</summary>
    public sealed record JsonSerializationFailed(string Message, Exception? InnerException = null) 
        : SerializationError;
    
    /// <summary>无效的数据结构</summary>
    public sealed record InvalidDataStructure(string Message) 
        : SerializationError;
    
    private SerializationError() { }
}

/// <summary>反序列化错误（和类型）</summary>
/// <remarks>
/// 表示配置反序列化过程中可能发生的错误。
/// 验证：需求 23.1-23.2, 23.5, 24.1-24.6
/// </remarks>
public abstract record DeserializationError
{
    /// <summary>JSON 反序列化失败</summary>
    public sealed record JsonDeserializationFailed(string Message, Exception? InnerException = null) 
        : DeserializationError;
    
    /// <summary>无效的 JSON 格式</summary>
    public sealed record InvalidJsonFormat(string Message) 
        : DeserializationError;
    
    /// <summary>缺失必需字段</summary>
    public sealed record MissingRequiredField(string FieldName) 
        : DeserializationError;
    
    /// <summary>类型不匹配</summary>
    public sealed record TypeMismatch(string ExpectedType, string ActualType) 
        : DeserializationError;
    
    /// <summary>损坏的配置文件</summary>
    public sealed record CorruptedConfig(string Message) 
        : DeserializationError;
    
    private DeserializationError() { }
}
