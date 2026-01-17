using LanguageExt;

namespace Machine.Framework.Core.Hardware.Interfaces
{
    public interface IValueCoercer
    {
         Fin<T> Coerce<T>(object? raw);
    }
}
