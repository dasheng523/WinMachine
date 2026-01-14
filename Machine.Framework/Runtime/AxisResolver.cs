using System;
using System.Linq;
using Machine.Framework.Devices.Motion.Abstractions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Configuration;
using static LanguageExt.Prelude;

namespace Machine.Framework.Runtime;

public interface IAxisResolver
{
    /// <summary>
    /// 解析逻辑轴名�?(控制器实�? 轴号)�?
    /// 逻辑轴名来自配置 System.AxisMap �?key（例�?X/Y1/Z1 等）�?
    /// </summary>
    Fin<(IMotionController<ushort, ushort, ushort> Controller, ushort Axis)> Resolve(string axisName);

    /// <summary>
    /// 仅解析轴号（强制�?Primary 板卡上）�?
    /// </summary>
    Fin<ushort> ResolveOnPrimary(string axisName);
}

/// <summary>
/// 通过配置把“机器逻辑轴”映射到“板卡名 + 轴号”�?
/// </summary>
public sealed class AxisResolver : IAxisResolver
{
    private readonly IMotionSystem _motionSystem;
    private readonly IOptions<SystemOptions> _options;

    public AxisResolver(IMotionSystem motionSystem, IOptions<SystemOptions> options)
    {
        _motionSystem = motionSystem ?? throw new ArgumentNullException(nameof(motionSystem));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Fin<(IMotionController<ushort, ushort, ushort> Controller, ushort Axis)> Resolve(string axisName)
    {
        if (string.IsNullOrWhiteSpace(axisName))
        {
            return FinFail<(IMotionController<ushort, ushort, ushort>, ushort)>(Error.New("axisName 不能为空"));
        }

        var map = _options.Value.AxisMap;
        if (map is null || map.Count == 0)
        {
            return FinFail<(IMotionController<ushort, ushort, ushort>, ushort)>(Error.New("未配�?System.AxisMap"));
        }

        var hit = map.TryGetValue(axisName, out var v)
            ? v
            : map.FirstOrDefault(kv => string.Equals(kv.Key, axisName, StringComparison.OrdinalIgnoreCase)).Value;

        if (hit is null)
        {
            return FinFail<(IMotionController<ushort, ushort, ushort>, ushort)>(Error.New($"未找到轴映射: {axisName}"));
        }

        if (string.IsNullOrWhiteSpace(hit.Board))
        {
            return FinSucc((_motionSystem.Primary, hit.Axis));
        }

        return
            from c in _motionSystem.GetBoard(hit.Board)
            select (c, hit.Axis);
    }

    public Fin<ushort> ResolveOnPrimary(string axisName) =>
        Resolve(axisName).Map(t => t.Axis);
}


