using System;
using System.Threading.Tasks;
using Machine.Framework.Core.Flow.Dsl;

namespace Machine.Framework.Interpreters.Flow
{
    /// <summary>
    /// 一个简单的 Flow 解释器，主要用于演示 AST 遍历与日志打印。
    /// 不调用实际硬件 SDK，而是通过日志输出动作。
    /// </summary>
    public interface IFlowInterpreter
    {
        Task<object> RunAsync(StepDesc definition);
    }

    public class SimpleLogInterpreter : IFlowInterpreter
    {
        private int _indent = 0;

        public async Task<object> RunAsync(StepDesc definition)
        {
            if (definition == null) return null;

            string indentStr = new string(' ', _indent * 2);
            Console.WriteLine($"{indentStr}[Step: {definition.Name}] Starting...");

            _indent++;
            object result = null;

            try
            {
                switch (definition)
                {
                    case ActionStepDesc action:
                        result = await ExecuteActionAsync(action, indentStr);
                        break;
                    case SequenceStepDesc sequence:
                        result = await ExecuteSequenceAsync(sequence);
                        break;
                    case MapStepDesc map:
                        result = await ExecuteMapAsync(map);
                        break;
                    case ScopeStepDesc scope:
                        result = await ExecuteScopeAsync(scope);
                        break;
                    default:
                        Console.WriteLine($"{indentStr}  !! Unknown step type: {definition.GetType().Name}");
                        break;
                }
            }
            finally
            {
                _indent--;
                Console.WriteLine($"{indentStr}[Step: {definition.Name}] Finished. Result: {result ?? "void"}");
            }

            return result;
        }

        private async Task<object> ExecuteActionAsync(ActionStepDesc action, string indent)
        {
            Console.WriteLine($"{indent}  -> EXEC: {action.Operation} on Device '{action.TargetDevice}'");
            if (action.Args != null && action.Args.Length > 0)
            {
                Console.WriteLine($"{indent}     ARGS: ({string.Join(", ", action.Args)})");
            }

            // 模拟 IO 延迟
            await Task.Delay(50);

            // 简单的 Mock 逻辑，支持测试分支
            if (action.Operation == "ReadAnalog") 
            {
                double mockValue = 55.5; 
                Console.WriteLine($"{indent}     MOCK RETURN: {mockValue}");
                return mockValue;
            }
            
            if (action.Operation == "CheckLevel") return true;
            if (action.Operation == "MoveTo") return true;
            if (action.Operation == "Fire") return new Unit();
            if (action.Operation == "NoOp") return new Unit();
            if (action.Operation == "Delay") return new Unit();

            return new Unit();
        }

        private async Task<object> ExecuteSequenceAsync(SequenceStepDesc sequence)
        {
            // 执行第一部分
            var firstResult = await RunAsync(sequence.First);
            
            // 使用第一部分的结果构造后续步骤
            var nextStepDef = sequence.NextFactory(firstResult);
            
            // 执行后续步骤
            var secondResult = await RunAsync(nextStepDef);

            // 合并结果 (SelectMany 的最终 resultSelector)
            return sequence.ResultSelector(firstResult, secondResult);
        }

        private async Task<object> ExecuteMapAsync(MapStepDesc map)
        {
            var sourceResult = await RunAsync(map.Source);
            return map.Mapper(sourceResult);
        }

        private async Task<object> ExecuteScopeAsync(ScopeStepDesc scope)
        {
            // Scope 目前只是一个包装
            return await RunAsync(scope.InnerStep);
        }
    }
}
