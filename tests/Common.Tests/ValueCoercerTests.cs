using System;
using Common.Core;
using Common.Hardware;
using FluentAssertions;
using LanguageExt;

namespace Common.Tests;

public class ValueCoercerTests
{
    private static T Expect<T>(Fin<T> fin) =>
        fin.Match(
            Succ: v => v,
            Fail: err => throw new Exception(err.ToString()));

    [Fact]
    public void CoerceLevel_ShouldSupportCommonTruthyFalsyStrings()
    {
        var c = new DefaultValueCoercer();

        Expect(c.Coerce<Level>("ON")).Should().Be(Level.On);
        Expect(c.Coerce<Level>("off")).Should().Be(Level.Off);
        Expect(c.Coerce<Level>("1")).Should().Be(Level.On);
        Expect(c.Coerce<Level>("0")).Should().Be(Level.Off);
        Expect(c.Coerce<Level>(" true ")).Should().Be(Level.On);
        Expect(c.Coerce<Level>("false")).Should().Be(Level.Off);
    }

    [Fact]
    public void CoerceLevel_ShouldTreatNonZeroNumbersAsOn()
    {
        var c = new DefaultValueCoercer();

        Expect(c.Coerce<Level>(0)).Should().Be(Level.Off);
        Expect(c.Coerce<Level>(2)).Should().Be(Level.On);
        Expect(c.Coerce<Level>(-1)).Should().Be(Level.On);
        Expect(c.Coerce<Level>(0.0)).Should().Be(Level.Off);
        Expect(c.Coerce<Level>(0.01)).Should().Be(Level.On);
    }

    [Fact]
    public void CoerceDouble_ShouldAllowLeadingNumberWithUnits()
    {
        var c = new DefaultValueCoercer();

        Expect(c.Coerce<double>("12.3kPa")).Should().BeApproximately(12.3, 1e-9);
        Expect(c.Coerce<double>("-7.5 bar")).Should().BeApproximately(-7.5, 1e-9);
    }
}
