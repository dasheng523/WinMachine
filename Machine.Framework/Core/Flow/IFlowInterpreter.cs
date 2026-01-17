using System.Threading.Tasks;
using Machine.Framework.Core.Flow.Dsl;

namespace Machine.Framework.Core.Flow
{
    /// <summary>
    /// Flow DSL 解释器/执行器核心契约
    /// </summary>
    public interface IFlowInterpreter
    {
        /// <summary>
        /// 异步运行给定的步骤描述，并返回执行结果。
        /// </summary>
        /// <param name="definition">步骤的抽象语法树描述</param>
        /// <param name="context">执行上下文，包含设备映射、配置和取消令牌</param>
        /// <returns>流程生成的结果对象 (对于 Unit 流程通常返回 Machine.Framework.Core.Flow.Dsl.Unit)</returns>
        Task<object?> RunAsync(StepDesc definition, FlowContext context);
    }
}
