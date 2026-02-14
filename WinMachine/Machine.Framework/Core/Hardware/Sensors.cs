using LanguageExt;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Hardware.Interfaces;
using static LanguageExt.Prelude;

namespace Machine.Framework.Core.Hardware
{
    public interface IRawSensor 
    {
         string Name { get; }
         Fin<object?> ReadRaw();
    }

    public sealed class CoercedSensor<T> : ISensor<T>
    {
        private readonly IRawSensor _raw;
        private readonly IValueCoercer _coercer;

        public CoercedSensor(IRawSensor raw, IValueCoercer coercer)
        {
            _raw = raw;
            _coercer = coercer;
        }

        public Fin<T> Read() => 
            _raw.ReadRaw().Bind(val => _coercer.Coerce<T>(val));
    }
    
    public sealed class DigitalInputSensor : ISensor<Level>, IRawSensor
    {
         public string Name { get; }
         private readonly IDigitalInput _input;
         
         public DigitalInputSensor(string name, IDigitalInput input)
         {
             Name = name;
             _input = input;
         }
         
         public Fin<Level> Read() => _input.Read();
         
         public Fin<object?> ReadRaw() => Read().Map(l => (object?)l);
    }
}
