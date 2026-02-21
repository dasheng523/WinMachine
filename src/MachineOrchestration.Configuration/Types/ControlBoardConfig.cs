using System.Text.Json.Serialization;

namespace MachineOrchestration.Configuration.Types;

/// <summary>控制板配置（和类型）</summary>
/// <remarks>
/// 使用 JsonDerivedType 属性支持多态序列化。
/// 验证：需求 10.1-10.5, 12.1-12.4
/// </remarks>
[JsonDerivedType(typeof(LeiSai), "leisai")]
[JsonDerivedType(typeof(ZhengYunDong), "zhengyundong")]
[JsonDerivedType(typeof(Simulated), "simulated")]
public abstract record ControlBoardConfig
{
    /// <summary>雷赛控制板配置</summary>
    public sealed record LeiSai(
        string Connection,
        LeiSaiParameters Parameters) : ControlBoardConfig;
    
    /// <summary>正运动控制板配置</summary>
    public sealed record ZhengYunDong(
        string Connection,
        ZhengYunDongParameters Parameters) : ControlBoardConfig;
    
    /// <summary>模拟控制板配置</summary>
    public sealed record Simulated(
        long LatencyMs) : ControlBoardConfig;
    
    private ControlBoardConfig() { }
}

/// <summary>雷赛控制板参数</summary>
public sealed record LeiSaiParameters(
    int MaxAxes,
    float DefaultSpeed);

/// <summary>正运动控制板参数</summary>
public sealed record ZhengYunDongParameters(
    int MaxAxes,
    float DefaultSpeed);
