using System;

namespace MachineOrchestration.ControlBoards.Types;

/// <summary>控制板错误（代数数据类型）</summary>
public abstract record ControlBoardError
{
    /// <summary>连接错误</summary>
    public sealed record ConnectionError(string Message, Exception? InnerException = null) 
        : ControlBoardError;
    
    /// <summary>命令执行失败</summary>
    public sealed record CommandFailed(string CommandDescription, string Reason, Exception? InnerException = null) 
        : ControlBoardError;
    
    /// <summary>控制板未初始化</summary>
    public sealed record NotInitialized(string Message = "Control board has not been initialized") 
        : ControlBoardError;
    
    private ControlBoardError() { }
}
