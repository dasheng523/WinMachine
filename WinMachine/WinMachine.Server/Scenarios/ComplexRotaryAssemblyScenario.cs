using System;
using Machine.Framework.Core.Configuration.Models;
using Unit = Machine.Framework.Core.Flow.Dsl.Unit;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Machine.Framework.Core.Blueprint;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Flow.Steps;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Interpreters.Configuration;
using Machine.Framework.Telemetry.Schema;
using Machine.Framework.Visualization;
using static Machine.Framework.Core.Flow.Steps.FlowBuilders;

namespace WinMachine.Server.Scenarios;

/// <summary>
/// 零件状态枚举，定义零件在组装流程中的生命周期状态
/// </summary>
public enum PartStatus
{
    /// <summary>空位，无零件</summary>
    Empty,
    /// <summary>新零件，待测试</summary>
    New,
    /// <summary>测试中</summary>
    Testing,
    /// <summary>测试完成</summary>
    Tested,
    /// <summary>旧零件，待下料</summary>
    Old
}

/// <summary>
/// 组装作业配置参数
/// </summary>
public record AssemblyJobConfig(
    string Name,
    CylinderID CylLift,
    AxisID AxisTable,
    CylinderID CylGrip,
    string VacSlide1,
    string VacSlide2,
    string VacTest1,
    string VacTest2,
    bool ExpectedSlidePos
);

/// <summary>
/// 复杂转盘组装场景实现
/// 模拟一个包含上料、测试、组装、下料等步骤的自动化生产线
/// </summary>
public sealed class ComplexRotaryAssemblyScenario : IScenarioFactory
{
    /// <summary>
    /// 场景名称
    /// </summary>
    public string Name => "复杂转盘组装场景 (核心逻辑版)";

    // --- 常量定义 ---
    // 测试时长常量（秒）
    private const double MIN_TEST_DURATION_SECONDS = 10;
    private const double MAX_TEST_DURATION_SECONDS = 20;

    // Feeder 位置常量（毫米）
    private const double FEEDER_UNLOAD_POSITION_MM = -40;
    private const double FEEDER_LOAD_POSITION_MM = 40;
    private const double FEEDER_PEN_DOWN_POSITION_MM = 50;
    private const double FEEDER_PEN_UP_POSITION_MM = -50;
    private const double FEEDER_PEN_HOME_POSITION_MM = 0;

    // 转台角度常量（度）
    private const double TABLE_FRONT_POSITION_DEG = 90;
    private const double TABLE_BACK_POSITION_DEG = -90;
    private const double TABLE_HOME_POSITION_DEG = 0;

    // 零件 ID 生成范围
    private const int MIN_PART_ID = 1000;
    private const int MAX_PART_ID = 9999;

    // 真空设备名称常量
    private const string SLIDE_VAC_1 = "Slide_Vac_1";
    private const string SLIDE_VAC_2 = "Slide_Vac_2";
    private const string SLIDE_VAC_3 = "Slide_Vac_3";
    private const string SLIDE_VAC_4 = "Slide_Vac_4";
    private const string TEST_VAC_L1 = "Test_Vac_L1";
    private const string TEST_VAC_L2 = "Test_Vac_L2";
    private const string TEST_VAC_R1 = "Test_Vac_R1";
    private const string TEST_VAC_R2 = "Test_Vac_R2";

    // --- 硬件定义 ---
    private readonly ComplexRotaryMachine _machine = new();

    // 线程安全的测试结束时间字典
    private readonly ConcurrentDictionary<string, DateTime> _testEndTimes = new();
    // 线程安全的随机数生成器
    private static readonly Random _rand = new();

    /// <summary>
    /// 安全解析零件状态，失败时返回默认状态
    /// </summary>
    private static PartStatus SafeParsePartStatus(string stateStr, PartStatus defaultStatus = PartStatus.Empty)
    {
        if (string.IsNullOrWhiteSpace(stateStr))
            return defaultStatus;

        return Enum.TryParse<PartStatus>(stateStr, out var status) ? status : defaultStatus;
    }

    /// <summary>
    /// 构建场景的 Web 模型架构
    /// </summary>
    /// <returns>Web 机器模型</returns>
    public WebMachineModel BuildSchema()
    {
        var (config, visualsModel, machineName) = _machine.Build();
        return WebMachineModelMapper.MapToWebModel(config, visualsModel, machineName);
    }

    /// <summary>
    /// 构建场景运行时
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>场景运行时实例</returns>
    public ScenarioRuntime BuildRuntime(CancellationToken ct)
    {
        var (config, visualsModel, machineName) = _machine.Build();
        var flow = BuildFlow();
        var ctx = new FlowContext(config, ct);
        var schema = WebMachineModelMapper.MapToWebModel(config, visualsModel, machineName);
        return new ScenarioRuntime(Name, schema.SchemaVersion ?? "1.0", ctx, flow, schema);
    }

    /// <summary>
    /// 管理测试流程
    /// 检查零件状态，启动测试或完成测试
    /// </summary>
    /// <param name="vacName">真空设备名称</param>
    /// <returns>测试流程步骤</returns>
    public Step<Unit> ManageTest(string vacName)
    {
        return Material(vacName).CheckState().SelectMany(stateStr =>
        {
            var state = SafeParsePartStatus(stateStr);
            if (state == PartStatus.New)
            {
                var testDuration = MIN_TEST_DURATION_SECONDS + _rand.NextDouble() * (MAX_TEST_DURATION_SECONDS - MIN_TEST_DURATION_SECONDS);
                _testEndTimes[vacName] = DateTime.Now.AddSeconds(testDuration);
                return Material(vacName).Transform(PartStatus.Testing.ToString());
            }
            if (state == PartStatus.Testing && _testEndTimes.TryGetValue(vacName, out var endTime) && DateTime.Now >= endTime)
            {
                return Scope($"测试完成 {vacName}", Material(vacName).Transform(PartStatus.Tested.ToString()));
            }
            return Step.NoOp();
        }, (a, b) => Unit.Default);
    }

    /// <summary>
    /// 执行上料/下料作业
    /// 根据滑台零件状态执行上料或下料操作
    /// </summary>
    /// <param name="vacSlide1">第一个滑台真空设备名称</param>
    /// <param name="vacSlide2">第二个滑台真空设备名称</param>
    /// <returns>上料/下料流程步骤</returns>
    public Step<Unit> FeederJob(string vacSlide1, string vacSlide2)
    {
        return
            from s1Str in Material(vacSlide1).CheckState()
            from s2Str in Material(vacSlide2).CheckState()
            let s1 = SafeParsePartStatus(s1Str)
            let s2 = SafeParsePartStatus(s2Str)
            from _act in (s1 != PartStatus.Testing || s2 != PartStatus.Testing) ? (
                from _x1 in Scope("Feeder: 下料位对齐", Motion(_machine.Axis_Feeder_X).MoveToAndWait(FEEDER_UNLOAD_POSITION_MM))
                from _1 in Scope("Feeder: 下料笔下降", Step.InParallel(Motion(_machine.Axis_Feeder_Z1).MoveToAndWait(FEEDER_PEN_DOWN_POSITION_MM), Motion(_machine.Axis_Feeder_Z2).MoveToAndWait(FEEDER_PEN_DOWN_POSITION_MM)))
                from _u1 in (s1 == PartStatus.Old ? Material(vacSlide1).AttachTo(_machine.Vac_Feeder_U1.Name, vacSlide1).Next(Material(vacSlide1).Consume()) : Step.NoOp())
                from _u2 in (s2 == PartStatus.Old ? Material(vacSlide2).AttachTo(_machine.Vac_Feeder_U2.Name, vacSlide2).Next(Material(vacSlide2).Consume()) : Step.NoOp())
                from _2 in Scope("Feeder: 下料笔回位", Step.InParallel(Motion(_machine.Axis_Feeder_Z1).MoveToAndWait(FEEDER_PEN_HOME_POSITION_MM), Motion(_machine.Axis_Feeder_Z2).MoveToAndWait(FEEDER_PEN_HOME_POSITION_MM)))
                from _x2 in Scope("Feeder: 上料位对齐", Motion(_machine.Axis_Feeder_X).MoveToAndWait(FEEDER_LOAD_POSITION_MM))
                from _3 in Scope("Feeder: 上料笔下降", Step.InParallel(Motion(_machine.Axis_Feeder_Z1).MoveToAndWait(FEEDER_PEN_UP_POSITION_MM), Motion(_machine.Axis_Feeder_Z2).MoveToAndWait(FEEDER_PEN_UP_POSITION_MM)))
                from _l1 in (s1 == PartStatus.Empty ? Material(vacSlide1).Spawn($"P_{_rand.Next(MIN_PART_ID, MAX_PART_ID)}", PartStatus.New.ToString()).Next(Material(_machine.Vac_Feeder_L1.Name).Detach()) : Step.NoOp())
                from _l2 in (s2 == PartStatus.Empty ? Material(vacSlide2).Spawn($"P_{_rand.Next(MIN_PART_ID, MAX_PART_ID)}", PartStatus.New.ToString()).Next(Material(_machine.Vac_Feeder_L2.Name).Detach()) : Step.NoOp())
                from _4 in Scope("Feeder: 上料笔回位", Step.InParallel(Motion(_machine.Axis_Feeder_Z1).MoveToAndWait(FEEDER_PEN_HOME_POSITION_MM), Motion(_machine.Axis_Feeder_Z2).MoveToAndWait(FEEDER_PEN_HOME_POSITION_MM)))
                from _refill in Scope("Feeder: 补充物料", Step.InParallel(Material(_machine.Vac_Feeder_L1.Name).Spawn("Src", PartStatus.New.ToString()), Material(_machine.Vac_Feeder_L2.Name).Spawn("Src", PartStatus.New.ToString())))
                select Unit.Default
            ) : Step.NoOp()
            select Unit.Default;
    }

    /// <summary>
    /// 执行组装作业
    /// 将测试完成的零件与新零件进行交换
    /// </summary>
    /// <param name="config">组装作业配置参数</param>
    /// <returns>组装流程步骤</returns>
    public Step<Unit> AssemblyJob(AssemblyJobConfig config)
    {
        return
            from _interlock in Cylinder(_machine.Cyl_Middle_Slide).WaitFor(config.ExpectedSlidePos)
            from _m1 in ManageTest(config.VacTest1)
            from _m2 in ManageTest(config.VacTest2)
            from sT1Str in Material(config.VacTest1).CheckState()
            from sT2Str in Material(config.VacTest2).CheckState()
            from sS1Str in Material(config.VacSlide1).CheckState()
            from sS2Str in Material(config.VacSlide2).CheckState()
            let sT1 = SafeParsePartStatus(sT1Str)
            let sT2 = SafeParsePartStatus(sT2Str)
            let sS1 = SafeParsePartStatus(sS1Str)
            let sS2 = SafeParsePartStatus(sS2Str)
            let needSwap = (sT1 == PartStatus.Tested || sT2 == PartStatus.Tested) && (sS1 == PartStatus.New || sS2 == PartStatus.New)
            from _act in needSwap ? Scope(config.Name,
                from _u1 in Cylinder(config.CylLift).FireAndWait(true)
                from _a1 in Motion(config.AxisTable).MoveToAndWait(config.ExpectedSlidePos ? TABLE_FRONT_POSITION_DEG : TABLE_BACK_POSITION_DEG)
                from _g1 in Cylinder(config.CylGrip).FireAndWait(false)
                from _pickO in Step.InParallel(
                    (sT1 == PartStatus.Tested ? Material(config.VacTest1).AttachTo(config.CylGrip.Name, config.VacTest1).Next(Material(config.VacTest1).Unbind()) : Step.NoOp()),
                    (sT2 == PartStatus.Tested ? Material(config.VacTest2).AttachTo(config.CylGrip.Name, config.VacTest2).Next(Material(config.VacTest2).Unbind()) : Step.NoOp())
                )
                from _a2 in Motion(config.AxisTable).MoveToAndWait(TABLE_HOME_POSITION_DEG)
                from _pickN in Step.InParallel(
                    (sS1 == PartStatus.New ? Material(config.VacSlide1).AttachTo(config.CylGrip.Name, config.VacSlide1).Next(Material(config.VacSlide1).Unbind()) : Step.NoOp()),
                    (sS2 == PartStatus.New ? Material(config.VacSlide2).AttachTo(config.CylGrip.Name, config.VacSlide2).Next(Material(config.VacSlide2).Unbind()) : Step.NoOp())
                )
                from _a3 in Motion(config.AxisTable).MoveToAndWait(config.ExpectedSlidePos ? TABLE_FRONT_POSITION_DEG : TABLE_BACK_POSITION_DEG)
                from _placeN in Step.InParallel(
                    (sS1 == PartStatus.New ? Material(config.VacTest1).Bind("Part", PartStatus.New.ToString()).Next(Material(config.CylGrip.Name).Detach()) : Step.NoOp()),
                    (sS2 == PartStatus.New ? Material(config.VacTest2).Bind("Part", PartStatus.New.ToString()).Next(Material(config.CylGrip.Name).Detach()) : Step.NoOp())
                )
                from _a4 in Motion(config.AxisTable).MoveToAndWait(TABLE_HOME_POSITION_DEG)
                from _placeO in Step.InParallel(
                    (sS1 == PartStatus.New && sT1 == PartStatus.Tested ? Material(config.VacSlide1).Bind("Part", PartStatus.Old.ToString()).Next(Material(config.CylGrip.Name).Detach()) : Step.NoOp()),
                    (sS2 == PartStatus.New && sT2 == PartStatus.Tested ? Material(config.VacSlide2).Bind("Part", PartStatus.Old.ToString()).Next(Material(config.CylGrip.Name).Detach()) : Step.NoOp())
                )
                from _g2 in Cylinder(config.CylGrip).FireAndWait(true)
                from _u2 in Cylinder(config.CylLift).FireAndWait(false)
                select Unit.Default
            ) : Step.NoOp()
            select Unit.Default;
    }

    /// <summary>
    /// 安全屏障检查
    /// 确保所有运动部件回到安全位置
    /// </summary>
    /// <returns>安全检查流程步骤</returns>
    public Step<Unit> SafetyBarrier() => Scope("安全检查", Step.InParallel(
        Motion(_machine.Axis_Feeder_X).MoveToAndWait(0),
        Motion(_machine.Axis_Feeder_Z1).MoveToAndWait(0),
        Motion(_machine.Axis_Feeder_Z2).MoveToAndWait(0),
        Cylinder(_machine.Cyl_R_Lift).WaitFor(false),
        Cylinder(_machine.Cyl_Lift_Right).WaitFor(false)
    ).Next(Step.NoOp()));

    /// <summary>
    /// 构建主流程
    /// 定义完整的组装循环流程
    /// </summary>
    /// <returns>流程描述</returns>
    private StepDesc BuildFlow()
    {
        var frontConfig = new AssemblyJobConfig(
            "FrontModule",
            _machine.Cyl_R_Lift,
            _machine.Axis_R_Table,
            _machine.Cyl_Grips_Left,
            SLIDE_VAC_1, SLIDE_VAC_2, TEST_VAC_L1, TEST_VAC_L2,
            true
        );

        var backConfig = new AssemblyJobConfig(
            "BackModule",
            _machine.Cyl_Lift_Right,
            _machine.Axis_Table_Right,
            _machine.Cyl_Grips_Right,
            SLIDE_VAC_3, SLIDE_VAC_4, TEST_VAC_R1, TEST_VAC_R2,
            false
        );

        var cycle = from _start in Scope("--- 循环开始 ---", Step.NoOp())
            from _init in Step.InParallel(Cylinder(_machine.Cyl_Grips_Left).FireAndWait(true), Cylinder(_machine.Cyl_Grips_Right).FireAndWait(true), Cylinder(_machine.Cyl_R_Lift).FireAndWait(false), Cylinder(_machine.Cyl_Lift_Right).FireAndWait(false))
            from _s1 in SafetyBarrier()
            from _m1 in Scope("滑台向前", Cylinder(_machine.Cyl_Middle_Slide).FireAndWait(true))
            from _w1 in Step.InParallel(AssemblyJob(frontConfig), FeederJob(SLIDE_VAC_3, SLIDE_VAC_4))
            from _s2 in SafetyBarrier()
            from _m2 in Scope("滑台向后", Cylinder(_machine.Cyl_Middle_Slide).FireAndWait(false))
            from _w2 in Step.InParallel(AssemblyJob(backConfig), FeederJob(SLIDE_VAC_1, SLIDE_VAC_2))
            select Unit.Default;
        return cycle.Loop().Definition;
    }


}
