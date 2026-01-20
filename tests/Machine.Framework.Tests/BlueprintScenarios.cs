using System;
using System.Linq;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Blueprint;

namespace Machine.Framework.Tests
{
    public static class BlueprintScenarios
    {
        public static IMachineBlueprintBuilder WinMachineWithDifferentialZ1()
        {
            var x = new AxisID("X");
            var z1 = new AxisID("Z1_Axis");

            return (from m in MachineBlueprint.Define("WinMachine_01")
                    from _1 in m.AddBoard("MainBoard", 0, board => {
                        board.AddAxis(x, 0, a => a.WithRange(0, 1000));
                        board.AddAxis(z1, 1, a => a.WithRange(0, 100));
                    }).Select(x => x)
                    select m);
        }

        public static IMachineBlueprintBuilder SimpleCylinderWithFeedback()
        {
            var clampId = new CylinderID("Clamp");

            return from m in MachineBlueprint.Define("WinMachine_02")
                   from _1 in m.AddBoard("IOBoard", 0, board => {
                       board.AddCylinder(clampId, 0, 1, c => c
                            .WithFeedback(0, 1)
                            .WithDynamics(100));
                   }).Select(x => x)
                   select m;
        }
    }
}

namespace Machine.Framework.Tests.BlueprintDsl
{
    internal static class LocalLinqExtensions
    {
        public static TResult Select<TSource, TResult>(this TSource source, Func<TSource, TResult> selector)
            => selector(source);

        public static TResult SelectMany<TSource, TIntermediate, TResult>(
            this TSource source,
            Func<TSource, TIntermediate> intermediateSelector,
            Func<TSource, TIntermediate, TResult> resultSelector)
        {
            var intermediate = intermediateSelector(source);
            return resultSelector(source, intermediate);
        }
    }
}
