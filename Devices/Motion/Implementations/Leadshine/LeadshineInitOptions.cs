using System.Collections.Generic;

namespace Devices.Motion.Implementations.Leadshine;

/// <summary>
/// 雷赛板卡初始化配置（用于把“旧项目初始化步骤”参数化）。
/// 该配置只在 LeadshineMotionController.Initialization() 期间应用。
/// </summary>
public sealed class LeadshineBoardInitOptions
{
    /// <summary>
    /// 是否在初始化完成后使能所有轴（0..AxisCount-1）。
    /// </summary>
    public bool EnableAllAxes { get; set; }

    /// <summary>
    /// 使能轴列表（当 EnableAllAxes=false 时使用）。
    /// </summary>
    public List<ushort> EnableAxes { get; set; } = [];

    /// <summary>
    /// 当 EnableAllAxes=true 时的轴数量。
    /// </summary>
    public ushort AxisCount { get; set; } = 8;

    /// <summary>
    /// 按轴的详细初始化参数。
    /// </summary>
    public List<LeadshineAxisInitOptions> Axes { get; set; } = [];
}

public sealed class LeadshineAxisInitOptions
{
    public ushort Axis { get; set; }

    public LeadshineElModeOptions? ElMode { get; set; }

    public LeadshineAlarmModeOptions? AlarmMode { get; set; }

    public LeadshinePulseOutModeOptions? PulseOutMode { get; set; }

    public LeadshineCounterOptions? Counter { get; set; }

    public double? Equiv { get; set; }

    public LeadshineHomeOptions? Home { get; set; }

    public List<LeadshineAxisIoMapOptions> IoMaps { get; set; } = [];
}

/// <summary>
/// 硬限位设置：对应 smc_set_el_mode(card, axis, el_mode, el_logic, el_action)
/// </summary>
public sealed class LeadshineElModeOptions
{
    public ushort Mode { get; set; }
    public ushort Logic { get; set; }
    public ushort Action { get; set; }
}

/// <summary>
/// 报警设置：对应 smc_set_alm_mode(card, axis, alm_mode, alm_logic, alm_action)
/// </summary>
public sealed class LeadshineAlarmModeOptions
{
    public ushort Mode { get; set; }
    public ushort Logic { get; set; }
    public ushort Action { get; set; }
}

/// <summary>
/// 脉冲输出模式：对应 smc_set_pulse_outmode(card, axis, mode)
/// </summary>
public sealed class LeadshinePulseOutModeOptions
{
    public ushort Mode { get; set; }
}

/// <summary>
/// 计数器输入模式 / 计数反向：对应 smc_set_counter_inmode + smc_set_counter_reverse
/// </summary>
public sealed class LeadshineCounterOptions
{
    public ushort InMode { get; set; }

    /// <summary>
    /// 0-不反向，1-反向。
    /// </summary>
    public ushort Reverse { get; set; }
}

public sealed class LeadshineHomeOptions
{
    /// <summary>
    /// 对应 smc_set_homemode(card, axis, home_mode, org_logic, ez_logic, home_dir)
    /// </summary>
    public LeadshineHomeModeOptions? Mode { get; set; }

    /// <summary>
    /// 对应 smc_set_home_profile_unit(card, axis, low, high, acc, dec)
    /// </summary>
    public LeadshineHomeProfileUnitOptions? ProfileUnit { get; set; }

    /// <summary>
    /// 对应 smc_set_home_pin_logic(card, axis, org_logic, ez_logic)
    /// </summary>
    public LeadshineHomePinLogicOptions? PinLogic { get; set; }
}

public sealed class LeadshineHomeModeOptions
{
    public ushort HomeMode { get; set; }
    public ushort OrgLogic { get; set; }
    public ushort EzLogic { get; set; }
    public ushort HomeDir { get; set; }
}

public sealed class LeadshineHomeProfileUnitOptions
{
    public double Low { get; set; }
    public double High { get; set; }
    public double Acc { get; set; }
    public double Dec { get; set; }
}

public sealed class LeadshineHomePinLogicOptions
{
    public ushort OrgLogic { get; set; }
    public ushort EzLogic { get; set; }
}

/// <summary>
/// 轴 IO 映射：对应 smc_set_axis_io_map(card, axis, io_type, map_index, io_no, logic)
/// </summary>
public sealed class LeadshineAxisIoMapOptions
{
    public ushort IoType { get; set; }
    public ushort MapIndex { get; set; }
    public ushort IoNo { get; set; }
    public ushort Logic { get; set; }
}
