using System;
using System.Linq;
using Machine.Framework.Core.Core;
using Leadshine;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Devices.Motion.Implementations.Leadshine;

public static class LeadshineInit
{
    public static Action<ushort> BuildInitDelegate(
        LeadshineBoardInitOptions options,
        Func<string, ushort>? axisNameResolver = null) =>
            cardNo =>
            {
                var r = Apply(cardNo, options, axisNameResolver);
                r.Match(
                    Succ: _ => unit,
                    Fail: e => throw new Exception($"Leadshine 初始化配置应用失�? {e.Message}"));
            };

    private static Fin<LUnit> Apply(ushort cardNo, LeadshineBoardInitOptions options, Func<string, ushort>? axisNameResolver) =>
        from _ in (options.Axes ?? []).Traverse(ax => ApplyAxis(cardNo, ax, axisNameResolver))
        from __ in ApplyEnableConfiguredAxes(cardNo, options, axisNameResolver)
        select unit;

    private static Fin<LUnit> ApplyAxis(ushort cardNo, LeadshineAxisInitOptions axis, Func<string, ushort>? axisNameResolver) =>
        from axisNo in ResolveAxisNo(axis, axisNameResolver)
        from _ in ApplyElMode(cardNo, axisNo, axis)
        from __ in ApplyAlarmMode(cardNo, axisNo, axis)
        from ___ in ApplyPulseOutMode(cardNo, axisNo, axis)
        from ____ in ApplyCounter(cardNo, axisNo, axis)
        from _____ in ApplyEquiv(cardNo, axisNo, axis)
        from ______ in ApplyHome(cardNo, axisNo, axis)
        from _______ in ApplyIoMaps(cardNo, axisNo, axis)
        select unit;

    private static Fin<LUnit> ApplyElMode(ushort cardNo, ushort axisNo, LeadshineAxisInitOptions axis)
    {
        if (axis.ElMode is null) return FinSucc(unit);

        return TryFin($"smc_set_el_mode(axis={axisNo})", () =>
            Check(LTSMC.smc_set_el_mode(cardNo, axisNo, axis.ElMode.Mode, axis.ElMode.Logic, axis.ElMode.Action)));
    }

    private static Fin<LUnit> ApplyAlarmMode(ushort cardNo, ushort axisNo, LeadshineAxisInitOptions axis)
    {
        if (axis.AlarmMode is null) return FinSucc(unit);

        return TryFin($"smc_set_alm_mode(axis={axisNo})", () =>
            Check(LTSMC.smc_set_alm_mode(cardNo, axisNo, axis.AlarmMode.Mode, axis.AlarmMode.Logic, axis.AlarmMode.Action)));
    }

    private static Fin<LUnit> ApplyPulseOutMode(ushort cardNo, ushort axisNo, LeadshineAxisInitOptions axis)
    {
        if (axis.PulseOutMode is null) return FinSucc(unit);

        return TryFin($"smc_set_pulse_outmode(axis={axisNo})", () =>
            Check(LTSMC.smc_set_pulse_outmode(cardNo, axisNo, axis.PulseOutMode.Mode)));
    }

    private static Fin<LUnit> ApplyCounter(ushort cardNo, ushort axisNo, LeadshineAxisInitOptions axis)
    {
        if (axis.Counter is null) return FinSucc(unit);

        return
            from _ in TryFin($"smc_set_counter_inmode(axis={axisNo})", () =>
                Check(LTSMC.smc_set_counter_inmode(cardNo, axisNo, axis.Counter.InMode)))
            from __ in TryFin($"smc_set_counter_reverse(axis={axisNo})", () =>
                Check(LTSMC.smc_set_counter_reverse(cardNo, axisNo, axis.Counter.Reverse)))
            select unit;
    }

    private static Fin<LUnit> ApplyEquiv(ushort cardNo, ushort axisNo, LeadshineAxisInitOptions axis)
    {
        if (axis.Equiv is null) return FinSucc(unit);

        return TryFin($"smc_set_equiv(axis={axisNo})", () =>
            Check(LTSMC.smc_set_equiv(cardNo, axisNo, axis.Equiv.Value)));
    }

    private static Fin<LUnit> ApplyHome(ushort cardNo, ushort axisNo, LeadshineAxisInitOptions axis)
    {
        if (axis.Home is null) return FinSucc(unit);

        var home = axis.Home;
        return
            from _ in (home.Mode is null
                ? FinSucc(unit)
                : TryFin($"smc_set_homemode(axis={axis.Axis})", () =>
                    Check(LTSMC.smc_set_homemode(cardNo, axisNo, home.Mode.HomeMode, home.Mode.OrgLogic, home.Mode.EzLogic, home.Mode.HomeDir))))
            from __ in (home.ProfileUnit is null
                ? FinSucc(unit)
                : TryFin($"smc_set_home_profile_unit(axis={axis.Axis})", () =>
                    Check(LTSMC.smc_set_home_profile_unit(cardNo, axisNo, home.ProfileUnit.Low, home.ProfileUnit.High, home.ProfileUnit.Acc, home.ProfileUnit.Dec))))
            from ___ in (home.PinLogic is null
                ? FinSucc(unit)
                : TryFin($"smc_set_home_pin_logic(axis={axis.Axis})", () =>
                    Check(LTSMC.smc_set_home_pin_logic(cardNo, axisNo, home.PinLogic.OrgLogic, home.PinLogic.EzLogic))))
            select unit;
    }

    private static Fin<LUnit> ApplyIoMaps(ushort cardNo, ushort axisNo, LeadshineAxisInitOptions axis)
    {
        if (axis.IoMaps is null || axis.IoMaps.Count == 0) return FinSucc(unit);

        return axis.IoMaps.Traverse(m =>
            TryFin($"smc_set_axis_io_map(axis={axisNo})", () =>
                Check(LTSMC.smc_set_axis_io_map(cardNo, axisNo, m.IoType, m.MapIndex, m.IoNo, m.Logic))));
    }

    private static Fin<LUnit> ApplyEnableConfiguredAxes(ushort cardNo, LeadshineBoardInitOptions options, Func<string, ushort>? axisNameResolver)
    {
        // 轴使能通常应是“运行时策略”，不建议配置化�?
        // 这里采用一个保守策略：如果配置里显式列出了要初始化的轴，则对这些轴统一使能一次�?
        // 若希望全机统一�?Enable 策略，请上移�?WinMachine 层（例如 MachineManager/MotionSystem）�?

        if (options.Axes is null || options.Axes.Count == 0) return FinSucc(unit);

        Fin<Arr<ushort>> axisNosFin = FinSucc(Arr<ushort>.Empty);
        foreach (var ax in options.Axes)
        {
            axisNosFin =
                from acc in axisNosFin
                from axisNo in ResolveAxisNo(ax, axisNameResolver)
                select acc.Add(axisNo);
        }

        return axisNosFin.Bind(axisNos =>
        {
            Fin<LUnit> enabled = FinSucc(unit);
            foreach (var axisNo in axisNos.Distinct())
            {
                enabled =
                    from _ in enabled
                    from __ in TryFin($"smc_write_sevon_pin(axis={axisNo})", () =>
                        Check(LTSMC.smc_write_sevon_pin(cardNo, axisNo, 1)))
                    select unit;
            }

            return enabled;
        });
    }

    private static Fin<ushort> ResolveAxisNo(LeadshineAxisInitOptions axis, Func<string, ushort>? axisNameResolver)
    {
        if (!string.IsNullOrWhiteSpace(axis.AxisName))
        {
            if (axisNameResolver is null)
            {
                return FinFail<ushort>(Error.New($"AxisName={axis.AxisName} 已配置，但未提供 axisNameResolver"));
            }

            return TryFinAxisResolve(axis.AxisName, axisNameResolver);
        }

        return FinSucc(axis.Axis);
    }

    private static Fin<ushort> TryFinAxisResolve(string axisName, Func<string, ushort> resolver)
    {
        try
        {
            return FinSucc(resolver(axisName));
        }
        catch (Exception ex)
        {
            return FinFail<ushort>(Error.New(new Exception($"AxisName 解析失败({axisName}): {ex.Message}", ex)));
        }
    }

    private static Fin<LUnit> TryFin(string op, Action action)
    {
        try
        {
            action();
            return FinSucc(unit);
        }
        catch (Exception ex)
        {
            return FinFail<LUnit>(Error.New(new Exception($"Leadshine.{op} 失败: {ex.Message}", ex)));
        }
    }

    private static void Check(short result)
    {
        if (result != 0)
        {
            throw new LeadshineException(result);
        }
    }
}


