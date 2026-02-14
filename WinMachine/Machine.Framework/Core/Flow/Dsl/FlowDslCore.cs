using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Machine.Framework.Core.Flow.Dsl
{
    // --- AST Nodes ---

    public enum Handling { Terminate, Retry, Skip, AskUser }

    public class StepPolicy
    {
        public int RetryCount { get; set; } = 0;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public Handling ErrorHandling { get; set; } = Handling.Terminate;
    }

    public abstract class StepDesc
    {
        public string Name { get; set; } = "Unnamed";
        public StepPolicy Policy { get; set; } = new StepPolicy();
    }

    public class ActionStepDesc : StepDesc
    {
        public required string TargetDevice { get; set; }
        public required string Operation { get; set; }
        public required object[] Args { get; set; }
    }

    public class SequenceStepDesc : StepDesc
    {
        public required StepDesc First { get; set; }
        // 关键点：用工厂函数存储后续步骤的生成逻辑，以支持 Data Dependency
        public required Func<object, StepDesc> NextFactory { get; set; }
        // 用于合并结果
        public required Func<object, object, object> ResultSelector { get; set; }
    }

    public class MapStepDesc : StepDesc
    {
        public required StepDesc Source { get; set; }
        public required Func<object, object> Mapper { get; set; }
    }

    public class ScopeStepDesc : StepDesc
    {
        public required StepDesc InnerStep { get; set; }
    }

    public class ParallelStepDesc : StepDesc
    {
        public required StepDesc[] Steps { get; set; }
    }

    public class LoopStepDesc : StepDesc
    {
        public required StepDesc InnerStep { get; set; }
        public int Count { get; set; } = -1; // -1 for infinite
    }

    // --- Fluent API Wrapper ---

    public struct Unit 
    { 
        public static Unit Default => new Unit();
        public override string ToString() => "()";
    }

    public class Step<T>
    {
        public StepDesc Definition { get; }
        public Step(StepDesc definition) => Definition = definition;
    }

    public static class Step
    {
        public static Step<Unit> Start() => new Step<Unit>(new ActionStepDesc 
        { 
            Name = "Start", 
            TargetDevice = "System", 
            Operation = "NoOp", 
            Args = Array.Empty<object>() 
        });

        public static Step<object[]> InParallel(params Step<Unit>[] steps)
        {
            return new Step<object[]>(new ParallelStepDesc
            {
                Name = "ParallelGroup",
                Steps = Array.ConvertAll(steps, s => s.Definition)
            });
        }

        public static Step<T> Throw<T>(string message)
        {
            return new Step<T>(new ActionStepDesc
            {
                Name = "ThrowException",
                TargetDevice = "System",
                Operation = "Throw",
                Args = new object[] { message }
            });
        }
        public static Step<Unit> NoOp()
        {
             return new Step<Unit>(new ActionStepDesc
             {
                 Name = "NoOp",
                 TargetDevice = "System",
                 Operation = "NoOp",
                 Args = Array.Empty<object>()
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

        public static Step<TResult> SelectMany<TSource, TCollection, TResult>(
            this Step<TSource> source,
            Func<TSource, Step<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            var seq = new SequenceStepDesc
            {
                Name = "Sequence",
                First = source.Definition,
                NextFactory = (obj) => 
                {
                    var typedObj = (TSource)obj!;
                    var nextStep = collectionSelector(typedObj);
                    return nextStep.Definition; 
                },
                ResultSelector = (s, c) => resultSelector((TSource)(s ?? default(TSource)!), (TCollection)(c ?? default(TCollection)!))!
            };
            
            return new Step<TResult>(seq);
        }

        public static Step<TResult> Select<TSource, TResult>(
            this Step<TSource> source,
            Func<TSource, TResult> selector)
        {
            var map = new MapStepDesc
            {
                Name = "Map",
                Source = source.Definition,
                Mapper = (obj) => selector((TSource)obj!)!
            };
            return new Step<TResult>(map); 
        }

        public static Step<T> Where<T>(this Step<T> source, Func<T, bool> predicate)
        {
            var seq = new SequenceStepDesc
            {
                Name = "Where",
                First = source.Definition,
                NextFactory = (obj) =>
                {
                    var val = (T)obj!;
                    if (predicate(val))
                    {
                        return new ActionStepDesc 
                        { 
                            Name = "WherePass", 
                            Operation = "NoOp", 
                            TargetDevice = "System", 
                            Args = Array.Empty<object>() 
                        };
                    }
                    else
                    {
                        return Step.Throw<Unit>($"Signal validation failed for value: {val}").Definition;
                    }
                },
                ResultSelector = (s, c) => (T)s!
            };
            return new Step<T>(seq);
        }

        public static Step<T> Loop<T>(this Step<T> step, int count = -1)
        {
            return new Step<T>(new LoopStepDesc
            {
                Name = "Loop",
                InnerStep = step.Definition,
                Count = count
            });
        }
    }
}
