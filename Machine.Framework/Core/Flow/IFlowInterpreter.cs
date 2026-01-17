using System.Threading;
using System.Threading.Tasks;
using Machine.Framework.Core.Flow.Models;
using LanguageExt;

namespace Machine.Framework.Core.Flow
{
    /// <summary>
    /// Flow DSL 解释器/执行器契约
    /// </summary>
    public interface IFlowInterpreter
    {
        Task<Fin<LanguageExt.Unit>> ExecuteAsync(StepDesc step, CancellationToken ct);
    }
}
