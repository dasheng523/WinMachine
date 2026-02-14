using System;
using System.Reactive.Linq;
using System.Threading;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Flow.Steps;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Interpreters.Configuration;
using Machine.Framework.Core.Primitives;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;
using static WinMachine.MachineDevices;

namespace WinMachine;

internal sealed record SimulationFlowScenario(
    string Name,
    Func<CancellationTokenSource, ScenarioRuntime> Create);

internal sealed record ScenarioRuntime(
    FlowContext Context,
    StepDesc Flow,
    Action<FlowContext, System.Reactive.Disposables.CompositeDisposable>? BeforeRun,
    IObservable<SimulationDomainEvent> DomainEvents,
    TransferStationModel? TransferModel);

internal static class SimulationFlowScenarios
{
    public static SimulationFlowScenario[] All =>
    [
        System_Initialization(),
        TransferStation_MultiGripper_Swap(),
        // 单体组件形状与动作测试
        Test_Shape_Cylinder_Slide(),
        Test_Shape_Cylinder_Elevator(),
        Test_Shape_Cylinder_Gripper(),
        Test_Shape_Cylinder_Suction(),
        Test_Shape_Axis_Linear(),
        Test_Shape_Axis_Rotary(),
        Rotary_Dual_Pair_Grippers(),
    ];

    private static SimulationFlowScenario System_Initialization() =>
        new(
            "System_Initialization",
            cts =>
            {
                var bp = DefineBlueprint();
                var config = BlueprintInterpreter.ToConfig(bp);
                var context = new FlowContext(config, cts.Token);
                
                var flow =
                    (from _ in Name("执行设备初始化").Next(
                        Step.InParallel(
                            Cylinder(C1_Left_Grip1).Fire(true),
                            Cylinder(C1_Left_Grip2).Fire(true),
                            Cylinder(C1_Left_Grip3).Fire(true),
                            Cylinder(C1_Left_Grip4).Fire(true),
                            Cylinder(C2_Right_Grip1).Fire(true),
                            Cylinder(C2_Right_Grip2).Fire(true),
                            Cylinder(C2_Right_Grip3).Fire(true),
                            Cylinder(C2_Right_Grip4).Fire(true),
                            Motion(Z1_Lift).MoveToAndWait(0),
                            Motion(Z2_Lift).MoveToAndWait(0),
                            Cylinder(SlideCyl).Fire(false)
                        ))
                     select Unit.Default).Definition;

                return new ScenarioRuntime(context, flow, null, Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static IMachineBlueprintBuilder DefineBlueprint()
    {
         return MachineBlueprint.Define("WinMachine_Turret_Swap")
                    .AddBoard("Main", 0, b => b
                        .UseSimulator()
                        
                        // --- 基础运动部件定义 ---
                        .AddCylinder(SlideCyl, 0, 0, c => c.WithDynamics(800).Horizontal()) // 滑台
                        
                        // 左侧部件
                        .AddAxis(Z1_Lift, 1, a => a.WithRange(0, 200).Vertical())
                        .AddAxis(R1_Rotate, 2, a => a.WithRange(0, 360))
                        .AddCylinder(C1_Left_Grip1, 10, 10)
                        .AddCylinder(C1_Left_Grip2, 11, 11)
                        .AddCylinder(C1_Left_Grip3, 12, 12)
                        .AddCylinder(C1_Left_Grip4, 13, 13)

                        // 右侧部件
                        .AddAxis(Z2_Lift, 3, a => a.WithRange(0, 200).Vertical())
                        .AddAxis(R2_Rotate, 4, a => a.WithRange(0, 360))
                        .AddCylinder(C2_Right_Grip1, 20, 20)
                        .AddCylinder(C2_Right_Grip2, 21, 21)
                        .AddCylinder(C2_Right_Grip3, 22, 22)
                        .AddCylinder(C2_Right_Grip4, 23, 23)
                    )

                    // 2. 定义机械层级 (Kinematics Mount)
                    .Mount("Machine", machine => machine
                        // 左侧塔结构
                        .Mount("Left_Tower_Assembly", tower => tower
                            .LinkTo(Z1_Lift) // 整个塔随 Z1 升降
                            .Mount("Left_Turret", turret => turret
                                .LinkTo(R1_Rotate) // 塔头随 R1 旋转
                                .Mount("L_Pos_0", g => g.LinkTo(C1_Left_Grip1).WithOffset(x: 50))
                                .Mount("L_Pos_90", g => g.LinkTo(C1_Left_Grip2).WithOffset(y: 50))
                                .Mount("L_Pos_180", g => g.LinkTo(C1_Left_Grip3).WithOffset(x: -50))
                                .Mount("L_Pos_270", g => g.LinkTo(C1_Left_Grip4).WithOffset(y: -50))
                            )
                        )
                        // 右侧塔结构
                        .Mount("Right_Tower_Assembly", tower => tower
                            .WithOffset(x: 400) // 平移右侧塔
                            .LinkTo(Z2_Lift)
                            .Mount("Right_Turret", turret => turret
                                .LinkTo(R2_Rotate)
                                .Mount("R_Pos_0", g => g.LinkTo(C2_Right_Grip1).WithOffset(x: 50))
                                .Mount("R_Pos_90", g => g.LinkTo(C2_Right_Grip2).WithOffset(y: 50))
                                .Mount("R_Pos_180", g => g.LinkTo(C2_Right_Grip3).WithOffset(x: -50))
                                .Mount("R_Pos_270", g => g.LinkTo(C2_Right_Grip4).WithOffset(y: -50))
                            )
                        )
                    );
    }

    private static SimulationFlowScenario TransferStation_MultiGripper_Swap() =>
        new(
            "TransferStation_MultiGripper_Swap",
            cts =>
            {
                // 1. 定义物理蓝图 (Blueprint)
                var blueprint = DefineBlueprint();

                var config = BlueprintInterpreter.ToConfig(blueprint);
                var context = new FlowContext(config, cts.Token);

                // 3. 定义可复用的子流程 (Sub-Flows)
                
                // 子流程：同时张开某侧所有夹爪
                Func<bool, Step<Unit>> OpenLeftGrippers = (isOpen) => 
                     Step.InParallel(
                         Cylinder(C1_Left_Grip1).Fire(isOpen),
                         Cylinder(C1_Left_Grip2).Fire(isOpen),
                         Cylinder(C1_Left_Grip3).Fire(isOpen),
                         Cylinder(C1_Left_Grip4).Fire(isOpen)
                     ).Select(_ => Unit.Default);

                Func<bool, Step<Unit>> OpenRightGrippers = (isOpen) => 
                     Step.InParallel(
                         Cylinder(C2_Right_Grip1).Fire(isOpen),
                         Cylinder(C2_Right_Grip2).Fire(isOpen),
                         Cylinder(C2_Right_Grip3).Fire(isOpen),
                         Cylinder(C2_Right_Grip4).Fire(isOpen)
                     ).Select(_ => Unit.Default);

                // 4. 定义主业务流程
                var flow =
                    (from _0 in Name("系统初始化").Next(
                        Step.InParallel(
                            OpenLeftGrippers(true),  // 先全部张开
                            OpenRightGrippers(true),
                            Motion(Z1_Lift).MoveToAndWait(0), // 升至高位
                            Motion(Z2_Lift).MoveToAndWait(0),
                            Cylinder(SlideCyl).Fire(false) // 滑台归位
                        ))
                     
                     // 第一阶段：滑台出料，塔台取料
                     from _1 in Name("滑台推出").Next(Cylinder(SlideCyl).FireAndWait(true))
                     from _1b in Name("等待滑台到位稳定").Next(SystemStep.Delay(200)) // 增加显式等待，确保动作时序
                     from _2 in Name("双塔旋转到位").Next(
                         Step.InParallel(
                            Motion(R1_Rotate).MoveToAndWait(0),
                            Motion(R2_Rotate).MoveToAndWait(0)
                         ))
                     from _3 in Name("双塔下降取料").Next(
                         Step.InParallel(
                            Motion(Z1_Lift).MoveToAndWait(150),
                            Motion(Z2_Lift).MoveToAndWait(150)
                         ))
                     from _4 in Name("所有夹爪闭合").Next(
                         Step.InParallel(
                            OpenLeftGrippers(false), // False = 闭合/夹紧
                            OpenRightGrippers(false)
                         ))
                     from _5 in Name("双塔安全升起").Next(
                         Step.InParallel(
                            Motion(Z1_Lift).MoveToAndWait(0),
                            Motion(Z2_Lift).MoveToAndWait(0)
                         ))
                     
                     // 第二阶段：旋转180度交换
                     from _6 in Name("旋转180度交换").Next(
                         Step.InParallel(
                            Motion(R1_Rotate).MoveToAndWait(180),
                            Motion(R2_Rotate).MoveToAndWait(180)
                         ))
                     
                     // 第三阶段：放料
                     from _7 in Name("双塔下降放料").Next(
                         Step.InParallel(
                            Motion(Z1_Lift).MoveToAndWait(150),
                            Motion(Z2_Lift).MoveToAndWait(150)
                         ))
                     from _8 in Name("放开夹爪").Next(
                         Step.InParallel(
                            OpenLeftGrippers(true),
                            OpenRightGrippers(true)
                         ))
                     from _9 in Name("双塔升起复位").Next(
                         Step.InParallel(
                            Motion(Z1_Lift).MoveToAndWait(0),
                            Motion(Z2_Lift).MoveToAndWait(0)
                         ))
                     
                     select Unit.Default).Definition;

                return new ScenarioRuntime(context, flow, null, Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static SimulationFlowScenario Test_Shape_Cylinder_Slide() =>
        new("气缸：横向滑台", cts => CreateSingleCylinderScenario(cts, Test_Slide, "气缸：横向滑台行动"));

    private static SimulationFlowScenario Test_Shape_Cylinder_Elevator() =>
        new("气缸：升降台", cts => CreateSingleCylinderScenario(cts, Test_Elevator, "气缸：升降台行动"));

    private static SimulationFlowScenario Test_Shape_Cylinder_Gripper() =>
        new("气缸：夹爪", cts => CreateSingleCylinderScenario(cts, Test_Gripper, "气缸：夹爪行动"));

    private static SimulationFlowScenario Test_Shape_Cylinder_Suction() =>
        new("气缸：吸笔", cts => CreateSingleCylinderScenario(cts, Test_Suction, "气缸：吸笔行动"));

    private static SimulationFlowScenario Test_Shape_Axis_Linear() =>
        new("马达：直线线性轴", cts => CreateSingleAxisScenario(cts, Test_Linear, "马达：线性移动", 150));

    private static SimulationFlowScenario Test_Shape_Axis_Rotary() =>
        new("马达：旋转工作台", cts => CreateSingleAxisScenario(cts, Test_Rotary, "马达：旋转移动", 90));

    private static SimulationFlowScenario Rotary_Dual_Pair_Grippers() =>
        new(
            "业务：旋转盘双对夹爪同步动作",
            cts =>
            {
                var bp = MachineBlueprint.Define("Test_Rotary_Grippers")
                    .AddBoard("Main", 0, b => b.UseSimulator()
                        .AddAxis(Test_Rotary, 0, a => a.WithRange(0, 360))
                        .AddCylinder(C1_Left_Grip1, 10, 10)
                        .AddCylinder(C1_Left_Grip2, 11, 11)
                        .AddCylinder(C2_Right_Grip1, 12, 12)
                        .AddCylinder(C2_Right_Grip2, 13, 13))
                    .Mount("Machine", m => m
                        .Mount("Disk", d => d
                            .LinkTo(Test_Rotary)
                            // 左右各安装一对 (左侧 2个, 右侧 2个) - 使用委托语法确保并列
                            .Mount("Left_Pair_1", g => g.LinkTo(C1_Left_Grip1).WithOffset(x: -80, y: -30))
                            .Mount("Left_Pair_2", g => g.LinkTo(C1_Left_Grip2).WithOffset(x: -80, y: 30))
                            .Mount("Right_Pair_1", g => g.LinkTo(C2_Right_Grip1).WithOffset(x: 80, y: -30))
                            .Mount("Right_Pair_2", g => g.LinkTo(C2_Right_Grip2).WithOffset(x: 80, y: 30))
                        )
                    );

                var config = BlueprintInterpreter.ToConfig(bp);
                var context = new FlowContext(config, cts.Token);

                // 定义同步动作：False 为闭合，True 为松开
                Func<bool, Step<Unit>> FireAll = (state) => Step.InParallel(
                    Cylinder(C1_Left_Grip1).FireAndWait(state),
                    Cylinder(C1_Left_Grip2).FireAndWait(state),
                    Cylinder(C2_Right_Grip1).FireAndWait(state),
                    Cylinder(C2_Right_Grip2).FireAndWait(state)
                ).Select(_ => Unit.Default);

                var flow = (from _1 in Name("4个夹爪同步闭合").Next(FireAll(false))
                            from _2 in Name("旋转盘旋转180度").Next(Motion(Test_Rotary).MoveToAndWait(180))
                            from _3 in Name("4个夹爪同步松开").Next(FireAll(true))
                            from _4 in Name("复位旋转盘").Next(Motion(Test_Rotary).MoveToAndWait(0))
                            select Unit.Default).Definition;

                return new ScenarioRuntime(context, flow, null, Observable.Empty<SimulationDomainEvent>(), null);
            });

    private static ScenarioRuntime CreateSingleCylinderScenario(CancellationTokenSource cts, CylinderID id, string stepName)
    {
        var bp = MachineBlueprint.Define($"Test_{id.Name}")
            .AddBoard("Main", 0, b => b.UseSimulator().AddCylinder(id, 0, 0, c => c.WithDynamics(800)))
            .Mount("Machine", m => m.Mount(id.Name, n => n.LinkTo(id)));
        
        var config = BlueprintInterpreter.ToConfig(bp);
        var context = new FlowContext(config, cts.Token);
        var flow = (from _ in Name(stepName).Next(Cylinder(id).FireAndWait(true))
                    from __ in Name("回退").Next(Cylinder(id).FireAndWait(false))
                    select Unit.Default).Definition;
        
        return new ScenarioRuntime(context, flow, null, Observable.Empty<SimulationDomainEvent>(), null);
    }

    private static ScenarioRuntime CreateSingleAxisScenario(CancellationTokenSource cts, AxisID id, string stepName, double target)
    {
        var bp = MachineBlueprint.Define($"Test_{id.Name}")
            .AddBoard("Main", 0, b => b.UseSimulator().AddAxis(id, 0, a => a.WithRange(0, 180).WithKinematics(200, 100)))
            .Mount("Machine", m => m.Mount(id.Name, n => n.LinkTo(id)));
        
        var config = BlueprintInterpreter.ToConfig(bp);
        var context = new FlowContext(config, cts.Token);
        var flow = (from _ in Name(stepName).Next(Motion(id).MoveToAndWait(target))
                    from __ in Name("回正").Next(Motion(id).MoveToAndWait(0))
                    select Unit.Default).Definition;
        
        return new ScenarioRuntime(context, flow, null, Observable.Empty<SimulationDomainEvent>(), null);
    }
}

internal static class StepNextExtensions
{
    public static Step<T> Next<TPrevious, T>(this Step<TPrevious> prev, Step<T> next)
    {
        return prev.SelectMany(_ => next, (_, n) => n);
    }
}
