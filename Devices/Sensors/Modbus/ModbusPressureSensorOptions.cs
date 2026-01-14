namespace Devices.Sensors.Modbus;

/// <summary>
/// 压力传感器（Modbus RTU）参数。
/// 约定与旧项目一致：
/// - Read: HoldingRegister address=0 count=2 (low word first)
/// - Reset: WriteSingleRegister address=0x000B value=4
/// - AlertLimit: WriteMultipleRegisters address=0x0020 (low/high)
/// - Point=1000 (把 int32 / 1000 -> kg)
/// </summary>
public sealed class ModbusPressureSensorOptions
{
    /// <summary>
    /// 从站 ID。
    /// </summary>
    public byte SlaveId { get; init; }

    /// <summary>
    /// 读取压力值的起始寄存器地址 (Holding Register)。
    /// 读取 2 个寄存器 (int32)。
    /// </summary>
    public ushort ReadAddress { get; init; } = 0;

    /// <summary>
    /// 清零操作的寄存器地址。
    /// 往该地址写入 4 即可清零。
    /// </summary>
    public ushort ResetAddress { get; init; } = 0x000B;

    /// <summary>
    /// 设置报警上限的寄存器地址。
    /// 写入双字 (int32)。
    /// </summary>
    public ushort AlertLimitAddress { get; init; } = 0x0020;

    /// <summary>
    /// 分辨率/比例因子。
    /// 读取到的整数值除以该值得到实际物理量 (如 kg)。
    /// </summary>
    public double Point { get; init; } = 1000.0;
}
