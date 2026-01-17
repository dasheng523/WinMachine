using LanguageExt;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface IDigitalInput 
    {
        Fin<Level> Read(); 
    }
    
    public static class DiExtensions
    {
        public static Fin<bool> ReadActive(this IDigitalInput input) =>
            input.Read().Map(l => l == Level.On);
    }
}
