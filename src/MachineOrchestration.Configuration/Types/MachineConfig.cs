using LanguageExt;
using MachineOrchestration.Core.Types;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Configuration.Types;

/// <summary>机器配置</summary>
/// <remarks>
/// 包含完整的机器定义、控制板配置和自动化逻辑集合。
/// 验证：需求 10.1-10.5, 12.1-12.4, 14.1-14.5
/// </remarks>
public sealed record MachineConfig(
    ComposableEntity Machine,
    ControlBoardConfig ControlBoard,
    HashMap<LogicId, AutomationLogic> AutomationLogics)
{
    /// <summary>创建空的机器配置</summary>
    public static MachineConfig Empty(ComposableEntity machine, ControlBoardConfig controlBoard) =>
        new(machine, controlBoard, HashMap<LogicId, AutomationLogic>());
}
