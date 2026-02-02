using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Framework.Telemetry.Schema;

namespace WinMachine.Server.Scenarios;

internal sealed class ScenarioRegistry
{
    private readonly Dictionary<string, IScenarioFactory> _scenarios;

    public ScenarioRegistry()
    {
        var list = new IScenarioFactory[]
        {
            new ComplexRotaryAssemblyScenario()
        };

        _scenarios = list.ToDictionary(s => s.Name, s => s, StringComparer.Ordinal);
    }

    public IReadOnlyCollection<string> Names => _scenarios.Keys;

    public bool TryGet(string name, out IScenarioFactory? scenario)
    {
        scenario = null;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return _scenarios.TryGetValue(name, out scenario);
    }

    public IScenarioFactory Get(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Scenario name is required.", nameof(name));

        if (_scenarios.TryGetValue(name, out var s)) return s;

        throw new KeyNotFoundException($"Unknown scenario '{name}'. Known: {string.Join(", ", _scenarios.Keys.OrderBy(x => x))}");
    }

    public WebMachineModel BuildSchema(string name) => Get(name).BuildSchema();

    public ScenarioRuntime BuildRuntime(string name, CancellationToken ct) => Get(name).BuildRuntime(ct);
}

public interface IScenarioFactory
{
    string Name { get; }

    WebMachineModel BuildSchema();

    ScenarioRuntime BuildRuntime(CancellationToken ct);
}

public sealed record ScenarioRuntime(
    string ScenarioName,
    string SchemaVersion,
    Machine.Framework.Core.Flow.FlowContext Context,
    Machine.Framework.Core.Flow.Dsl.StepDesc Flow,
    Machine.Framework.Telemetry.Schema.WebMachineModel Schema);
