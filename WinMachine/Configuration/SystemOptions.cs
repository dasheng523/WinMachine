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
}
