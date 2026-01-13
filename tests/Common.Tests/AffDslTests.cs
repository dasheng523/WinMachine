using System;
using System.Threading.Tasks;
using Common.Effects;
using FluentAssertions;
using LanguageExt;
using static LanguageExt.Prelude;
using Xunit;

namespace Common.Tests;

public sealed class AffDslTests
{
    [Fact]
    public async Task Aff_Linq_Dsl_ShouldComposeAndReturnValue()
    {
        // DSL 外观示例：用 LINQ query syntax 组合可失败的步骤
        // （这相当于 Haskell 的 do-notation）
        Aff<int> program =
            from _ in MachineAff.Sleep(10)
            from __ in MachineAff.Guard(condition: true, messageIfFalse: "should-not-fail")
            from x in MachineAff.Pure(21)
            select x * 2;

        var fin = await program.Run();

        fin.IsSucc.Should().BeTrue(fin.ToString());
        fin.IfFail(0).Should().Be(42);
    }

    [Fact]
    public async Task Aff_Linq_Dsl_ShouldShortCircuitOnFailure()
    {
        Aff<int> program =
            from _ in MachineAff.Ok
            from __ in MachineAff.Guard(condition: false, messageIfFalse: "boom")
            from x in MachineAff.Pure(123) // 不应执行
            select x;

        var fin = await program.Run();

        fin.IsFail.Should().BeTrue(fin.ToString());
        fin.Match(Succ: _ => false, Fail: e => e.Message.Contains("boom")).Should().BeTrue();
    }
}
