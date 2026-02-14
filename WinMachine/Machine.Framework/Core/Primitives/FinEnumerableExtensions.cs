using System;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Core.Primitives;

public static class FinEnumerableExtensions
{
    public static Fin<LUnit> Traverse<T>(this IEnumerable<T> source, Func<T, Fin<LUnit>> effect)
    {
        foreach (var item in source)
        {
            var r = effect(item);
            if (r.IsFail)
            {
                return r;
            }
        }

        return FinSucc(unit);
    }
}


