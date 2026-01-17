using System;

namespace Machine.Framework.Core.Flow.Dsl
{
    // ----------------------------------------------------------------
    // 1. 核心定义 (AST - Abstract Syntax Tree)
    // ----------------------------------------------------------------
    
    // 策略定义：容错与介入
    public enum Handling
    {
        Terminate, // 终止 (默认)
        Retry,     // 自动重试
        Skip,      // 跳过/忽略错误
        AskUser    // 人工介入
    }

    public class StepPolicy
    {
        public int RetryCount { get; set; } = 0;
        public TimeSpan? Timeout { get; set; }
        public Handling ErrorHandling { get; set; } = Handling.Terminate;

        public StepPolicy Clone()
        {
            return new StepPolicy 
            { 
                RetryCount = RetryCount, 
                Timeout = Timeout, 
                ErrorHandling = ErrorHandling 
            };
        }
    }

    // 步骤描述基类 (纯数据，不含执行逻辑)
    public abstract class StepDesc
    {
        public string Name { get; set; } = "Unnamed";
        public StepPolicy Policy { get; set; } = new StepPolicy();
    }

    // 原子操作 (Action)
    public class ActionStepDesc : StepDesc
    {
        public required string TargetDevice { get; set; }
        public required string Operation { get; set; }
        public required object[] Args { get; set; }
    }

    // 复合操作 (Scope/Sequence)
    public class ScopeStepDesc : StepDesc
    {
        // 实际上 LINQ 构造的是一个嵌套结构，或者是一个平铺的 Sequence。
        // 为了简化 AST，我们假设 Scope 内部是一个独立的 StepDesc (通常是 SequenceStepDesc)
        public required StepDesc InnerStep { get; set; }
    }

    // 序列操作 (由 SelectMany 产生)
    public class SequenceStepDesc : StepDesc
    {
        public required StepDesc First { get; set; }
        public required Func<object, StepDesc> NextFactory { get; set; } // 惰性构造下一不
    }

    // ----------------------------------------------------------------
    // 2. DSL 构建器 (Builder / Monad)
    // ----------------------------------------------------------------

    // 泛型构建器，用于 LINQ 推导
    public class Step<T>
    {
        public StepDesc Definition { get; }

        public Step(StepDesc definition)
        {
            Definition = definition;
        }

        public Step<T> Configure(Action<StepDesc> configure)
        {
            configure(Definition);
            return this;
        }
    }

    // ----------------------------------------------------------------
    // 3. DSL 入口与扩展 (Static Entry Definitions)
    // ----------------------------------------------------------------

    // 简化的 Unit 类型
    public struct Unit { }

    public static class Step
    {
        public static Step<Unit> Name(string name)
        {
            return new Step<Unit>(new ActionStepDesc { Name = name, Operation = "NoOp", TargetDevice = "System", Args = Array.Empty<object>() });
        }

        // Scope 接受一个已经构造好的子流程 (subFlow)
        public static Step<T> Scope<T>(string name, Step<T> subFlow)
        {
            var scope = new ScopeStepDesc
            {
                Name = name,
                InnerStep = subFlow.Definition
            };
            return new Step<T>(scope);
        }

        // 抛出异常步骤，用于逻辑分支中的失败路径
        public static Step<T> Throw<T>(string message)
        {
            return new Step<T>(new ActionStepDesc 
            { 
                Name = "Throw", 
                Operation = "Throw", 
                TargetDevice = "System",
                Args = new object[] { message } 
            });
        }
    }

    public static class StepExtensions
    {
        // --- Policies ---

        public static Step<T> Retry<T>(this Step<T> step, int count)
        {
            step.Definition.Policy.RetryCount = count;
            return step;
        }

        public static Step<T> WithTimeout<T>(this Step<T> step, int ms)
        {
            step.Definition.Policy.Timeout = TimeSpan.FromMilliseconds(ms);
            return step;
        }

        public static Step<T> OnError<T>(this Step<T> step, Handling handling)
        {
            step.Definition.Policy.ErrorHandling = handling;
            return step;
        }

        // --- LINQ Magic ---

        // Standard SelectMany for 'from x in a from y in b select y'
        public static Step<TResult> SelectMany<TSource, TCollection, TResult>(
            this Step<TSource> source,
            Func<TSource, Step<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            // 这里我们构造一个 Sequence 节点
            // 注意：因为是 Definition 阶段，Func 并不会立即执行，或者我们需要像 Expression Expression Tree 那样处理
            // 但为了简化，我们通常假设 Sequence 是运行时的逻辑链接。
            // 在 Definition 阶段，我们实际上是在构建一个链表或树。
            
            // 下面的实现稍微有点 trick：
            // 我们不能真正获得 TSource 的值（因为它在运行时产生），
            // 除非我们处于运行时 Monad。
            // 但 DSL 描述通常需要构造完整的静态树吗？
            // 如果 DSL 包含 `if (prevResult)` 这种逻辑，那么树的结构是动态的。
            // Rx.NET 的 SelectMany 是支持这种动态性的。
            // 所以我们的 Definition 必须包含这个 Factory。

            var seq = new SequenceStepDesc
            {
                Name = "Sequence",
                First = source.Definition,
                // 下面这个 Lambda 在 Config 阶段无法被完全展开，
                // 除非我们提供一个 Mock 上下文。
                // *这是解释器模式的关键*：Definition 本身可能包含 Lambda。
                NextFactory = (obj) => 
                {
                    var typedObj = (TSource)obj;
                    var nextStep = collectionSelector(typedObj);
                    return nextStep.Definition; 
                }
            };
            
            // 对于结果选择器，我们也需要封装。
            // 但为了简化 DSL AST，我们暂时返回 nextStep 的类型作为 Step<TResult>
            // 实际上 ResultSelector 的结果通常只影响返回值，不影响副作用。
            // 这是一个简化实现，用于演示结构。
            
            return new Step<TResult>(seq);
        }

        // Simple Select for 'select x' at the end
        public static Step<TResult> Select<TSource, TResult>(
            this Step<TSource> source,
            Func<TSource, TResult> selector)
        {
            // Select 在步骤流中通常意味着映射结果，不产生新步骤
            // 我们可以包装一个 MapStepDesc，或者忽略它（如果结果不重要）
            // 为了演示，直接返回包装。
            return new Step<TResult>(source.Definition); 
        }
    }
}
