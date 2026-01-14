using LanguageExt;

namespace Machine.Framework.Core.Hardware;

public sealed class CoercedSensor<T> : ISensor<T>
{
    private readonly IRawSensor _raw;
    private readonly IValueCoercer _coercer;

    public CoercedSensor(string name, IRawSensor raw, IValueCoercer coercer)
    {
        Name = name;
        _raw = raw;
        _coercer = coercer;
    }

    public string Name { get; }

    public Fin<T> Read() =>
        _raw.ReadRaw().Bind(_coercer.Coerce<T>);
}

public sealed class DigitalInputSensor : ISensor<Machine.Framework.Core.Core.Level>, IRawSensor
{
    private readonly IDigitalInput _input;

    public DigitalInputSensor(string name, IDigitalInput input)
    {
        Name = name;
        _input = input;
    }

    public string Name { get; }

    public Fin<Machine.Framework.Core.Core.Level> Read() => _input.Read();

    public Fin<object?> ReadRaw() => Read().Map(x => (object?)x);
}


