namespace WinMachine.Configuration;

/// <summary>
/// 系统级配置
/// </summary>
public class SystemOptions
{
    /// <summary>
    /// 是否使用模拟器
    /// </summary>
    public bool UseSimulator { get; set; }

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
    /// 多板卡配置（可为 1 块、2 块或更多）。
    /// 为空/未配置时，回退使用上面的单板字段。
    /// </summary>
    public List<MotionBoardOptions> MotionBoards { get; set; } = [];
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
}
