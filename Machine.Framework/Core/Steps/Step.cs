using System;
using System.Reactive.Linq;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Core.Steps
{
    public static class Step
    {
        public static IObservable<Fin<LUnit>> Run(Action action)
        {
            return Observable.Start(() => 
            {
                try { action(); return FinSucc(unit); }
                catch (Exception ex) { return FinFail<LUnit>(Error.New(ex)); }
            });
        }
        
        // Placeholder Attempt to satisfy compiler if referenced
        public static IObservable<Fin<LUnit>> Attempt(int retries, Func<IObservable<Fin<LUnit>>> flow)
        {
             return flow().Retry(retries);
        }
    }
}
