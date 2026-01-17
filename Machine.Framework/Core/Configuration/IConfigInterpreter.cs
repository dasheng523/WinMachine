using System;
using Machine.Framework.Core.Configuration.Models;

namespace Machine.Framework.Core.Configuration
{
    /// <summary>
    /// DSL 解释器契约
    /// </summary>
    /// <typeparam name="TResult">解释执行后的产物类型</typeparam>
    public interface IConfigInterpreter<out TResult>
    {
        TResult Interpret(MachineConfig config);
    }
}
