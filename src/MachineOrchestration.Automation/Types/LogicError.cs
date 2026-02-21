using System;

namespace MachineOrchestration.Automation.Types;

/// <summary>逻辑管理错误（和类型 - Sum Type）</summary>
/// <remarks>
/// 表示自动化逻辑管理过程中可能发生的错误。
/// 使用代数数据类型确保类型安全。
/// 验证：需求 14.1-14.3, 24.2-24.3
/// </remarks>
public abstract record LogicError
{
    /// <summary>逻辑已存在</summary>
    public sealed record LogicAlreadyExists(LogicId Id, string Message) : LogicError
    {
        public LogicAlreadyExists(LogicId id) 
            : this(id, $"Logic with ID {id} already exists") { }
    }
    
    /// <summary>逻辑未找到</summary>
    public sealed record LogicNotFound(LogicId Id, string Message) : LogicError
    {
        public LogicNotFound(LogicId id) 
            : this(id, $"Logic with ID {id} not found") { }
    }
    
    /// <summary>无效的逻辑名称</summary>
    public sealed record InvalidLogicName(string Name, string Message) : LogicError
    {
        public InvalidLogicName(string name) 
            : this(name, $"Invalid logic name: '{name}' (cannot be null or whitespace)") { }
    }
    
    /// <summary>无效的 AST</summary>
    public sealed record InvalidAst(string Message) : LogicError
    {
        public InvalidAst() 
            : this("AST cannot be null") { }
    }
    
    /// <summary>存储操作失败</summary>
    public sealed record StorageOperationFailed(string Message, Exception? InnerException = null) : LogicError;
    
    // 私有构造函数确保只能通过上述变体创建实例
    private LogicError() { }
}
