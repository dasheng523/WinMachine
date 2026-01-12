using System.Text.Json.Serialization;

namespace WinMachine.Configuration;

/// <summary>
/// 系统级配置
/// </summary>
public class SystemOptions
{
    public static readonly IReadOnlyList<string> DefaultSuggestedAxisKeys =
    [
        "X", "Y1", "Y2", "Z1", "Z2",
        "P1", "P2",
        "L1", "L2", "L3", "L4",
        "R1", "R2",
        "RS1", "RS2"
    ];

    /// <summary>
    /// 是否使用模拟器
    /// </summary>
    public bool UseSimulator { get; set; }

    /// <summary>
    /// AxisMap 的 Key 建议列表（仅用于 UI 提示，不影响解析逻辑）。
    /// </summary>
    public List<string> SuggestedAxisKeys { get; set; } = DefaultSuggestedAxisKeys.ToList();

    /// <summary>
    /// 便于在 UI 中编辑 SuggestedAxisKeys 的字符串形式（逗号/分号/换行分隔）。
    /// 实际保存仍以 SuggestedAxisKeys 为准。
    /// </summary>
    [JsonIgnore]
    public string SuggestedAxisKeysCsv
    {
        get => string.Join(",", SuggestedAxisKeys ?? []);
        set
        {
            var parts = (value ?? string.Empty)
                .Split([',', ';', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            SuggestedAxisKeys = parts.Count == 0 ? DefaultSuggestedAxisKeys.ToList() : parts;
        }
    }

    /// <summary>
    /// 多板卡配置（可为 1 块、2 块或更多）。
    /// 约定至少配置 1 块板卡（如 Main）。
    /// </summary>
    public List<MotionBoardOptions> MotionBoards { get; set; } = [];

    /// <summary>
    /// 逻辑轴名到 (板卡名 + 轴号) 的映射。
    /// 建议 key 使用：X/Y1/Y2/Z1/Z2/P1/P2/L1..L4/R1/R2/RS1/RS2 等。
    /// </summary>
    public Dictionary<string, AxisRefOptions> AxisMap { get; set; } = [];

    /// <summary>
    /// 逻辑 IO 映射（DI/DO）。
    /// Key 建议使用层级名，例如："ScanSeat.CylValve"、"ScanSeat.Extended"。
    /// </summary>
    public IoMapOptions IoMap { get; set; } = new();

    /// <summary>
    /// 逻辑气缸映射（单电磁阀）。
    /// Key 建议使用层级名，例如："ScanSeat.Cyl"。
    /// </summary>
    public Dictionary<string, SingleSolenoidCylinderOptions> CylinderMap { get; set; } = [];

    /// <summary>
    /// 逻辑传感器映射（来源可为 DI / Modbus / 串口等）。
    /// Key 建议使用层级名，例如："ScanSeat.Extended"、"Vacuum.Ok"、"Scanner.Main"。
    /// 说明：当前主要用于 DSL 外观定型；具体通讯实现可后续接入。
    /// </summary>
    public Dictionary<string, SensorOptions> SensorMap { get; set; } = [];
}

public sealed class AxisRefOptions
{
    /// <summary>
    /// 板卡名（对应 MotionBoards[*].Name）。为空则使用 Primary。
    /// </summary>
    public string? Board { get; set; }

    /// <summary>
    /// 板卡上的物理轴号。
    /// </summary>
    public ushort Axis { get; set; }
}

public sealed class IoMapOptions
{
    public Dictionary<string, IoRefOptions> Di { get; set; } = [];

    public Dictionary<string, IoRefOptions> Do { get; set; } = [];
}

public sealed class IoRefOptions
{
    /// <summary>
    /// 板卡名（对应 MotionBoards[*].Name）。为空则使用 Primary。
    /// </summary>
    public string? Board { get; set; }

    /// <summary>
    /// 板卡上的 DI/DO 点位号。
    /// </summary>
    public ushort Bit { get; set; }
}

public sealed class SingleSolenoidCylinderOptions
{
    /// <summary>
    /// 电磁阀输出（逻辑 DO 名）。
    /// </summary>
    public string ValveDo { get; set; } = string.Empty;

    /// <summary>
    /// 约定：电磁阀输出为 On 时表示 Extend 或 Retract。
    /// </summary>
    public Common.Hardware.CylinderCommand OnMeans { get; set; } = Common.Hardware.CylinderCommand.Extend;

    /// <summary>
    /// 可选：伸出到位（逻辑 DI 名，高有效）。
    /// </summary>
    public string? ExtendedDi { get; set; }

    /// <summary>
    /// 可选：缩回到位（逻辑 DI 名，高有效）。
    /// </summary>
    public string? RetractedDi { get; set; }

    /// <summary>
    /// 可选：健康/气压 OK（逻辑 DI 名，高有效）。
    /// </summary>
    public string? HealthOkDi { get; set; }
}

public enum SensorKind
{
    DiLevel = 0,
    ModbusCoil = 1,
    ModbusHoldingRegister = 2,
    SerialLine = 3
}

public sealed class SensorOptions
{
    public SensorKind Kind { get; set; }

    /// <summary>
    /// 当 Kind=DiLevel 时，引用逻辑 DI 名。
    /// </summary>
    public string? Di { get; set; }

    /// <summary>
    /// 当 Kind=Modbus* 时使用。
    /// </summary>
    public ModbusSensorOptions? Modbus { get; set; }

    /// <summary>
    /// 当 Kind=SerialLine 时使用（例如扫码器）。
    /// </summary>
    public SerialLineSensorOptions? Serial { get; set; }
}

public sealed class ModbusSensorOptions
{
    public string PortName { get; set; } = string.Empty;
    public int BaudRate { get; set; } = 115200;

    public byte SlaveId { get; set; }

    /// <summary>
    /// Coil/HoldingRegister 地址。
    /// </summary>
    public ushort Address { get; set; }

    /// <summary>
    /// HoldingRegister 数量（例如 float 可能需要 2）。
    /// </summary>
    public ushort Count { get; set; } = 1;
}

public sealed class SerialLineSensorOptions
{
    public string PortName { get; set; } = string.Empty;
    public int BaudRate { get; set; } = 115200;

    /// <summary>
    /// 可选：同一串口多设备时用于区分。
    /// </summary>
    public string? Address { get; set; }
}

public class MotionBoardOptions
{
    public string Name { get; set; } = "Main";

    /// <summary>
    /// 控制器类型：ZMotion, Leadshine, Simulator
    /// </summary>
    public string ControllerType { get; set; } = "Simulator";

    /// <summary>
    /// 控制器 IP 地址
    /// </summary>
    public string DeviceIp { get; set; } = "127.0.0.1";

    /// <summary>
    /// 控制器卡号/站号 (如雷赛 CardNo)
    /// </summary>
    public ushort DeviceCardNo { get; set; } = 0;

    /// <summary>
    /// 雷赛板卡初始化参数（仅当 ControllerType=Leadshine 时生效）。
    /// 用于配置：轴正反限位、报警、脉冲模式、脉冲当量、寻零等。
    /// </summary>
    public Devices.Motion.Implementations.Leadshine.LeadshineBoardInitOptions? LeadshineInit { get; set; }
}
