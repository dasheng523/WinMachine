using LanguageExt;
using Machine.Framework.Core.Primitives;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface ISingleSolenoidCylinder 
    {
        Fin<LUnit> Set(Level level);
        Fin<Level> Get();
    }
}
