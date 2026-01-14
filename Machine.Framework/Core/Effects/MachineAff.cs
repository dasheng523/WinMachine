using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Core.Effects;

/// <summary>
/// 基于 LanguageExt Aff 的最�?DSL�?
/// - 错误类型使用 LanguageExt.Machine.Framework.Core.Error
/// - �?LINQ query syntax 组合动作（do-notation 外观�?
/// </summary>
public static class MachineAff
{
    public static Aff<Unit> Ok => SuccessAff(unit);

    public static Aff<A> Pure<A>(A value) => SuccessAff(value);

    public static Aff<A> Fail<A>(string message) => FailAff<A>(Error.New(message));

    public static Aff<A> Fail<A>(Error error) => FailAff<A>(error);

    public static Aff<A> FromFin<A>(Func<Fin<A>> f) =>
        Aff(async () =>
        {
            try
            {
                var fin = f();
                return fin.Match(
                    Succ: a => a,
                    Fail: e => throw e.ToException());
            }
            catch (Exception ex)
            {
                throw Error.New(ex).ToException();
            }
        });

    public static Aff<Unit> Sleep(TimeSpan delay) =>
        Aff(async () =>
        {
            await Task.Delay(delay).ConfigureAwait(false);
            return unit;
        });

    public static Aff<Unit> Sleep(int milliseconds) => Sleep(TimeSpan.FromMilliseconds(milliseconds));

    public static Aff<Unit> Guard(bool condition, string messageIfFalse) =>
        condition ? Ok : Fail<Unit>(messageIfFalse);
}


