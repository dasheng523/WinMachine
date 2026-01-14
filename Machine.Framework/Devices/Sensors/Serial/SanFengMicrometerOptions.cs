namespace Machine.Framework.Devices.Sensors.Serial;

public sealed class SanFengMicrometerOptions
{
    public string PortName { get; init; } = string.Empty;

    public int BaudRate { get; init; } = 2400;

    public int ReadTimeoutMs { get; init; } = 500;

    public string NewLine { get; init; } = "\r";

    /// <summary>
    /// 触发一次测量（旧代码写�?"1"）�?
    /// </summary>
    public string TriggerCommand { get; init; } = "1";

    /// <summary>
    /// 最大重试次数（旧代码为 10）�?
    /// </summary>
    public int MaxAttempts { get; init; } = 10;
}


