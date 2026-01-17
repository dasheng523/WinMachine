using System;
using System.Linq;
using Machine.Framework.Core.Simulation;

using Machine.Framework.Tests.BlueprintDsl;

namespace Machine.Framework.Tests
{
    public static class BlueprintScenarios
    {
        // ... (methods remain the same)
        public static ISimulatorAssemblyBuilder WinMachineWithDifferentialZ1()
        {
            return from m in MachineSimulator.Assemble("WinMachine_01")
                   let mainBoard = m.AddBoard("MainBoard", cardId: 0)
                   let z1Axis = mainBoard.AddAxis(id: 1, name: "Z1_Axis")
                   let beam = m.Mount("MainBeam").AttachedTo(mainBoard.AddAxis(0, "X"))
                   let penL = m.Mount("PenLoading").AttachedTo(beam).LinkTo(z1Axis).WithTransform(z => z)
                   let penU = m.Mount("PenUnloading").AttachedTo(beam).LinkTo(z1Axis).WithTransform(z => -z)
                   select m;
        }

        public static ISimulatorAssemblyBuilder SimpleCylinderWithFeedback()
        {
            return from m in MachineSimulator.Assemble("WinMachine_02")
                   let board = m.AddBoard("IOBoard", cardId: 0)
                   let clamp = board.AddCylinder("Clamp", doOut: 0, doIn: 1)
                                    .WithFeedback(diOut: 0, diIn: 1)
                                    .WithDynamics(actionTimeMs: 100)
                   select m;
        }
    }
}

namespace Machine.Framework.Tests.BlueprintDsl
{
    /// <summary>
    /// 仅供蓝图定义的 LINQ 桥接。
    /// </summary>
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
