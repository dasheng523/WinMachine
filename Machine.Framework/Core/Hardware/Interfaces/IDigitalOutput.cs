using LanguageExt;
using Machine.Framework.Core.Primitives;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface IDigitalOutput 
    {
        Fin<LUnit> Write(Level level);
    }
}
