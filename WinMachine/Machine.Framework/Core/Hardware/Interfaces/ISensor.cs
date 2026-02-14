using LanguageExt;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface ISensor<T> 
    {
        Fin<T> Read();
    }
}
