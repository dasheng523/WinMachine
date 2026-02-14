using LanguageExt;
using Machine.Framework.Core.Primitives;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface IAxis
    {
        string Name { get; }
        Fin<double> GetCommandPos();
        Fin<double> GetEncoderPos();
        Fin<LUnit> MoveAbs(double pos);
        Fin<LUnit> Stop();
    }
}
