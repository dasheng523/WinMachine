using LanguageExt;
using Machine.Framework.Core.Core;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Core.Hardware
{
    public interface IAxis
    {
        string Name { get; }
        Fin<double> GetCommandPos();
        Fin<double> GetEncoderPos();
        Fin<LUnit> MoveAbs(double pos);
        Fin<LUnit> Stop();
    }

    public interface IDigitalInput 
    {
        Fin<Level> Read(); 
    }
    
    public static class DiExtensions
    {
        public static Fin<bool> ReadActive(this IDigitalInput input) =>
            input.Read().Map(l => l == Level.On);
    }

    public interface IDigitalOutput 
    {
        Fin<LUnit> Write(Level level);
    }
    
    public interface ISensor<T> 
    {
        Fin<T> Read();
    }
    
    public interface ISingleSolenoidCylinder 
    {
        Fin<LUnit> Set(Level level);
        Fin<Level> Get();
    }

    public interface IResolver<T>
    {
        Fin<T> Resolve(string name);
    }

    // Specialized resolvers used in HardwareFacade
    public interface IAxisResolver
    {
         Fin<(Machine.Framework.Devices.Motion.Abstractions.IMotionController<ushort, ushort, ushort> Controller, ushort Axis)> Resolve(string name);
         Fin<ushort> ResolveOnPrimary(string name);
    }

    public interface IIoResolver 
    {
         Fin<IDigitalInput> ResolveDi(string name);
         Fin<IDigitalOutput> ResolveDo(string name);
    }

    public interface ICylinderResolver 
    {
        Fin<ISingleSolenoidCylinder> Resolve(string name);
    }

    public interface IValueCoercer
    {
         Fin<T> Coerce<T>(object? raw);
    }
}
