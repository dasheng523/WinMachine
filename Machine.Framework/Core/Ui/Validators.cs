using System;
using System.Net;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Machine.Framework.Core.Ui;

public static class Validators
{
    public static Func<string, Fin<Unit>> NotEmptyFin(string fieldName) =>
        v => string.IsNullOrWhiteSpace(v)
            ? FinFail<Unit>(Error.New($"{fieldName} 不能为空"))
            : FinSucc(unit);

    public static Fin<Unit> IpFin(string ip) =>
        IPAddress.TryParse(ip, out _)
            ? FinSucc(unit)
            : FinFail<Unit>(Error.New("IP 地址格式不正确"));
}
