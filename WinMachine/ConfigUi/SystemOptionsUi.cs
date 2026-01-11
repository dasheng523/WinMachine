using LanguageExt;
using static LanguageExt.Prelude;
using Common.Ui;
using WinMachine.Configuration;
using Devices.Motion.Implementations.Leadshine;
using WinMachine.ConfigUi;

namespace WinMachine.ConfigUi;

public static class SystemOptionsUi
{
    private static readonly string[] ControllerTypes = ["ZMotion", "Leadshine", "Simulator"];

    private static readonly string[] SuggestedAxisKeys =
    [
        "X", "Y1", "Y2", "Z1", "Z2",
        "P1", "P2",
        "L1", "L2", "L3", "L4",
        "R1", "R2",
        "RS1", "RS2"
    ];

    public static Ui<FormSpec<SystemOptions>> Spec() =>
        UI.Form<SystemOptions>(
            from _page in UI.Page("系统配置",
                UI.Tabs(
                    UI.Tab("系统",
                        from _sec in UI.Section("运行模式",
                            from _g in UI.Grid(2,
                                UI.Label("使用模拟器"),
                                UI.Field<SystemOptions, bool>(m => m.UseSimulator).AsCheckBox()
                            )
                            select unit
                        )
                        select unit
                    ),
                    UI.Tab("板卡",
                        from _sec in UI.Section("MotionBoards",
                            from _help in UI.Help("至少配置 1 块板卡（如 Main）。连接信息统一在此处配置。")
                            from _list in UI.List<SystemOptions, MotionBoardOptions>(
                                get: m => m.MotionBoards,
                                itemKey: b => b.Name,
                                itemUi: i =>
                                    from _item in UI.Section($"板卡 #{i}",
                                        from _g in UI.Grid(2,
                                            UI.Field<MotionBoardOptions, string>(b => b.Name)
                                                .AsTextBox()
                                                .Validate(Validators.NotEmptyFin("Name"))
                                                .Labeled("Name"),

                                            UI.Field<MotionBoardOptions, string>(b => b.ControllerType)
                                                .AsCombo(ControllerTypes)
                                                .Validate(Validators.NotEmptyFin("ControllerType"))
                                                .Labeled("控制器类型"),

                                            UI.Field<MotionBoardOptions, string>(b => b.DeviceIp)
                                                .AsTextBox()
                                                .Validate(Validators.IpFin)
                                                .Labeled("IP 地址"),

                                            UI.Field<MotionBoardOptions, ushort>(b => b.DeviceCardNo)
                                                .AsUInt16()
                                                .Labeled("卡号/站号")
                                        )
                                        from _ls in UI.When<MotionBoardOptions>(
                                            b => string.Equals(b.ControllerType, "Leadshine", StringComparison.OrdinalIgnoreCase),
                                            UI.OptionalObject<MotionBoardOptions, LeadshineBoardInitOptions>(
                                                b => b.LeadshineInit,
                                                title: "LeadshineInit",
                                                body: LeadshineInitUi.Editor(),
                                                initiallyExpanded: true,
                                                defaultEnabled: true
                                            )
                                        )
                                        select unit
                                    )
                                    select unit
                            )
                            select unit
                        )
                        select unit
                    ),
                    UI.Tab("轴映射",
                        from _sec in UI.Section("AxisMap",
                            from _help in UI.Help("Key 建议使用：X/Y1/Y2/Z1/Z2/P1/P2/L1..L4/R1/R2/RS1/RS2")
                            from _map in UI.Dictionary<SystemOptions, AxisRefOptions>(
                                get: m => m.AxisMap,
                                keyUi: key => UI.Key(current: key, suggested: SuggestedAxisKeys, allowFreeText: true),
                                valueUi: a =>
                                    UI.Grid(2,
                                        UI.Label("Board"),
                                        UI.Field<AxisRefOptions, string?>(x => x.Board)
                                            .AsCombo(root =>
                                            {
                                                var sys = (SystemOptions)root;
                                                var names = (sys.MotionBoards ?? [])
                                                    .Select(b => b.Name)
                                                    .Where(n => !string.IsNullOrWhiteSpace(n))
                                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                                    .ToList();

                                                // 空字符串表示 Primary/默认板卡
                                                names.Insert(0, "");
                                                return names;
                                            }),

                                        UI.Label("Axis"),
                                        UI.Field<AxisRefOptions, ushort>(x => x.Axis).AsUInt16()
                                    )
                            )
                            select unit
                        )
                        select unit
                    )
                )
            )
            select unit
        );
}
