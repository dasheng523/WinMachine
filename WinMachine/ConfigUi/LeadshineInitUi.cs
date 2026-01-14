using System;
using Machine.Framework.Devices.Motion.Implementations.Leadshine;
using LanguageExt;
using static LanguageExt.Prelude;
using Machine.Framework.Core.Ui;

namespace WinMachine.ConfigUi;

public static class LeadshineInitUi
{
    public static Ui<Unit> Editor() =>
        from _axes in UI.Section("Axes",
            UI.List<LeadshineBoardInitOptions, LeadshineAxisInitOptions>(
                get: b => b.Axes,
                itemKey: ax => string.IsNullOrWhiteSpace(ax.AxisName) ? ax.Axis.ToString() : ax.AxisName,
                itemUi: i =>
                    from _sec in UI.Section($"Axis #{i}", AxisEditor())
                    select unit
            ))
        select unit;

    private static Ui<Unit> AxisEditor() =>
        from _core in UI.Grid(2,
            UI.Field<LeadshineAxisInitOptions, ushort>(x => x.Axis)
                .AsUInt16()
                .Labeled("Axis"),

            UI.Field<LeadshineAxisInitOptions, string?>(x => x.AxisName)
                .AsTextBox("逻辑轴名，可�?)
                .Labeled("AxisName"),

            UI.Field<LeadshineAxisInitOptions, double?>(x => x.Equiv)
                .AsTextBox("脉冲当量，可�?)
                .Labeled("Equiv")
        )
        from _optional in UI.VStack(
            UI.OptionalObject<LeadshineAxisInitOptions, LeadshineElModeOptions>(
                x => x.ElMode,
                title: "ElMode",
                body: ElModeEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            ),

            UI.OptionalObject<LeadshineAxisInitOptions, LeadshineAlarmModeOptions>(
                x => x.AlarmMode,
                title: "AlarmMode",
                body: AlarmModeEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            ),

            UI.OptionalObject<LeadshineAxisInitOptions, LeadshinePulseOutModeOptions>(
                x => x.PulseOutMode,
                title: "PulseOutMode",
                body: PulseOutModeEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            ),

            UI.OptionalObject<LeadshineAxisInitOptions, LeadshineCounterOptions>(
                x => x.Counter,
                title: "Counter",
                body: CounterEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            ),

            UI.OptionalObject<LeadshineAxisInitOptions, LeadshineHomeOptions>(
                x => x.Home,
                title: "Home",
                body: HomeEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            ),

            UI.Section("IoMaps",
                UI.List<LeadshineAxisInitOptions, LeadshineAxisIoMapOptions>(
                    get: x => x.IoMaps,
                    itemKey: m => $"{m.IoType}:{m.MapIndex}",
                    itemUi: i =>
                        from _sec in UI.Section($"IoMap #{i}", IoMapEditor())
                        select unit
                ))
        )
        select unit;

    private static Ui<Unit> ElModeEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshineElModeOptions, ushort>(x => x.Mode).AsUInt16().Labeled("Mode"),
            UI.Field<LeadshineElModeOptions, ushort>(x => x.Logic).AsUInt16().Labeled("Logic"),
            UI.Field<LeadshineElModeOptions, ushort>(x => x.Action).AsUInt16().Labeled("Action")
        )
        select unit;

    private static Ui<Unit> AlarmModeEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshineAlarmModeOptions, ushort>(x => x.Mode).AsUInt16().Labeled("Mode"),
            UI.Field<LeadshineAlarmModeOptions, ushort>(x => x.Logic).AsUInt16().Labeled("Logic"),
            UI.Field<LeadshineAlarmModeOptions, ushort>(x => x.Action).AsUInt16().Labeled("Action")
        )
        select unit;

    private static Ui<Unit> PulseOutModeEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshinePulseOutModeOptions, ushort>(x => x.Mode).AsUInt16().Labeled("Mode")
        )
        select unit;

    private static Ui<Unit> CounterEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshineCounterOptions, ushort>(x => x.InMode).AsUInt16().Labeled("InMode"),
            UI.Field<LeadshineCounterOptions, ushort>(x => x.Reverse).AsUInt16().Labeled("Reverse")
        )
        select unit;

    private static Ui<Unit> HomeEditor() =>
        from _ in UI.VStack(
            UI.OptionalObject<LeadshineHomeOptions, LeadshineHomeModeOptions>(
                x => x.Mode,
                title: "Mode",
                body: HomeModeEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            ),
            UI.OptionalObject<LeadshineHomeOptions, LeadshineHomeProfileUnitOptions>(
                x => x.ProfileUnit,
                title: "ProfileUnit",
                body: HomeProfileUnitEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            ),
            UI.OptionalObject<LeadshineHomeOptions, LeadshineHomePinLogicOptions>(
                x => x.PinLogic,
                title: "PinLogic",
                body: HomePinLogicEditor(),
                initiallyExpanded: false,
                defaultEnabled: false
            )
        )
        select unit;

    private static Ui<Unit> HomeModeEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshineHomeModeOptions, ushort>(x => x.HomeMode).AsUInt16().Labeled("HomeMode"),
            UI.Field<LeadshineHomeModeOptions, ushort>(x => x.OrgLogic).AsUInt16().Labeled("OrgLogic"),
            UI.Field<LeadshineHomeModeOptions, ushort>(x => x.EzLogic).AsUInt16().Labeled("EzLogic"),
            UI.Field<LeadshineHomeModeOptions, ushort>(x => x.HomeDir).AsUInt16().Labeled("HomeDir")
        )
        select unit;

    private static Ui<Unit> HomeProfileUnitEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshineHomeProfileUnitOptions, double>(x => x.Low).AsTextBox().Labeled("Low"),
            UI.Field<LeadshineHomeProfileUnitOptions, double>(x => x.High).AsTextBox().Labeled("High"),
            UI.Field<LeadshineHomeProfileUnitOptions, double>(x => x.Acc).AsTextBox().Labeled("Acc"),
            UI.Field<LeadshineHomeProfileUnitOptions, double>(x => x.Dec).AsTextBox().Labeled("Dec")
        )
        select unit;

    private static Ui<Unit> HomePinLogicEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshineHomePinLogicOptions, ushort>(x => x.OrgLogic).AsUInt16().Labeled("OrgLogic"),
            UI.Field<LeadshineHomePinLogicOptions, ushort>(x => x.EzLogic).AsUInt16().Labeled("EzLogic")
        )
        select unit;

    private static Ui<Unit> IoMapEditor() =>
        from _ in UI.Grid(2,
            UI.Field<LeadshineAxisIoMapOptions, ushort>(x => x.IoType).AsUInt16().Labeled("IoType"),
            UI.Field<LeadshineAxisIoMapOptions, ushort>(x => x.MapIndex).AsUInt16().Labeled("MapIndex"),
            UI.Field<LeadshineAxisIoMapOptions, ushort>(x => x.IoNo).AsUInt16().Labeled("IoNo"),
            UI.Field<LeadshineAxisIoMapOptions, ushort>(x => x.Logic).AsUInt16().Labeled("Logic")
        )
        select unit;
}


