using System;
using System.Linq;
using Machine.Framework.Core.Core;
using Machine.Framework.Core.Hardware;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Configuration;
using static LanguageExt.Prelude;

namespace Machine.Framework.Runtime;

public interface ICylinderResolver
{
    Fin<ISingleSolenoidCylinder> Resolve(string name);
}

public sealed class CylinderResolver : ICylinderResolver
{
    private readonly IIoResolver _io;
    private readonly IOptions<SystemOptions> _options;

    public CylinderResolver(IIoResolver io, IOptions<SystemOptions> options)
    {
        _io = io ?? throw new ArgumentNullException(nameof(io));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Fin<ISingleSolenoidCylinder> Resolve(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return FinFail<ISingleSolenoidCylinder>(Error.New("气缸名称不能为空"));
        }

        var map = _options.Value.CylinderMap;
        if (map is null || map.Count == 0)
        {
            return FinFail<ISingleSolenoidCylinder>(Error.New("未配�?System.CylinderMap"));
        }

        if (!map.TryGetValue(name, out var opt))
        {
            opt = map.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
        }

        if (opt is null)
        {
            return FinFail<ISingleSolenoidCylinder>(Error.New($"未找到气缸映�? {name}"));
        }

        if (string.IsNullOrWhiteSpace(opt.ValveDo))
        {
            return FinFail<ISingleSolenoidCylinder>(Error.New($"气缸 {name} 未配�?ValveDo"));
        }

        return
            from valve in _io.ResolveDo(opt.ValveDo)
            from extended in ResolveDiAsLevelSensor(opt.ExtendedDi)
            from retracted in ResolveDiAsLevelSensor(opt.RetractedDi)
            from health in ResolveDiAsLevelSensor(opt.HealthOkDi)
            select (ISingleSolenoidCylinder)new SingleSolenoidCylinder(
                name,
                valve,
                opt.OnMeans,
                extended,
                retracted,
                health,
                pressureSensor: Option<ISensor<double>>.None);
    }

    private Fin<Option<ISensor<Level>>> ResolveDiAsLevelSensor(string? diName)
    {
        if (string.IsNullOrWhiteSpace(diName))
        {
            return FinSucc(Option<ISensor<Level>>.None);
        }

        return _io.ResolveDi(diName)
            .Map(di => (ISensor<Level>)new DigitalInputSensor(diName, di))
            .Map(Some);
    }
}


