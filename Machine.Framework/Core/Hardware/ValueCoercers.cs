using System;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using Machine.Framework.Core.Hardware.Interfaces;

namespace Machine.Framework.Core.Hardware
{
    public sealed class DefaultValueCoercer : IValueCoercer
    {
        public Fin<T> Coerce<T>(object? raw)
        {
             if (raw is T t) return FinSucc(t);
             if (raw == null) return FinFail<T>(Error.New("Value is null"));
             
             try 
             {
                 return FinSucc((T)Convert.ChangeType(raw, typeof(T)));
             }
             catch (Exception ex)
             {
                 return FinFail<T>(Error.New($"Conversion failed: {ex.Message}"));
             }
        }
    }
}
