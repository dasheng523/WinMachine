using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using WinMachine.Server;
using Xunit;

namespace WinMachine.Server.Tests;

public sealed class TelemetryE2ETests
{
    private const string ScenarioName = "复杂转盘组装场景 (核心逻辑版)";

    [Fact]
    public async Task Scenarios_endpoint_returns_registered_scenarios()
    {
        await using var host = await TestServerHost.StartAsync();

        using var http = new HttpClient { BaseAddress = host.BaseUri };
        using var resp = await http.GetAsync("/api/machine/scenarios");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);

        var scenarios = doc.RootElement.EnumerateArray()
            .Select(x => x.GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        scenarios.Should().Contain(ScenarioName);
    }

    [Fact]
    public async Task Schema_endpoint_returns_rotary_lift_schema()
    {
        await using var host = await TestServerHost.StartAsync();

        using var http = new HttpClient { BaseAddress = host.BaseUri };
        using var resp = await http.GetAsync($"/api/machine/schema?name={Uri.EscapeDataString(ScenarioName)}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("schemaVersion", out var schemaVersion).Should().BeTrue();
        schemaVersion.GetString().Should().NotBeNullOrWhiteSpace();

        doc.RootElement.TryGetProperty("deviceRegistry", out var deviceRegistry).Should().BeTrue();
        deviceRegistry.ValueKind.Should().Be(JsonValueKind.Array);

        var ids = deviceRegistry.EnumerateArray()
            .Select(x => x.GetProperty("id").GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.Ordinal);

        // 对齐 ComplexRotaryMachine 里的核心设备 ID
        ids.Should().Contain(new[]
        {
            // 左右旋转模组
            "Cyl_R_Lift",
            "Axis_R_Table",
            "Cyl_Grips_Left",
            "Cyl_Lift_Right",
            "Axis_Table_Right",
            "Cyl_Grips_Right",
            // 中间搬运模组
            "Cyl_Middle_Slide",
            // 供料模组
            "Axis_Feeder_X",
            "Axis_Feeder_Z1",
            "Axis_Feeder_Z2",
            "Vac_Feeder_U1",
            "Vac_Feeder_L1",
        });

        // 验证物理属性映射 (Physical Property Mapping Verification)
        var deviceMap = deviceRegistry.EnumerateArray()
            .ToDictionary(
                x => x.GetProperty("id").GetString()!,
                x => x
            );

        // 1. 验证垂直导轨 (Vertical Linear Guide)
        // Axis_Feeder_Z1 被定义为: .Vertical().AsLinearGuide(100)
        deviceMap.Should().ContainKey("Axis_Feeder_Z1");
        var z1 = deviceMap["Axis_Feeder_Z1"];
        z1.GetProperty("type").GetString().Should().Be("LinearGuide");
        // 验证 Meta 中的物理参数
        z1.GetProperty("meta").TryGetProperty("isVertical", out var isVer).Should().BeTrue();
        isVer.GetBoolean().Should().BeTrue();
        z1.GetProperty("meta").TryGetProperty("length", out var len).Should().BeTrue();
        len.GetDouble().Should().Be(100);

        // 2. 验证旋转台 (Rotary Table)
        // Axis_R_Table 被定义为: .AsRotaryTable(50)
        deviceMap.Should().ContainKey("Axis_R_Table");
        var rTable = deviceMap["Axis_R_Table"];
        rTable.GetProperty("type").GetString().Should().Be("RotaryTable");
        rTable.GetProperty("meta").TryGetProperty("radius", out var rad).Should().BeTrue();
        rad.GetDouble().Should().Be(50);
    }

    [Fact]
    public async Task WebSocket_start_unknown_scenario_returns_error_then_flow_stopped()
    {
        await using var host = await TestServerHost.StartAsync();
        using var ws = await host.ConnectWebSocketAsync();

        await SendJsonAsync(ws, new { cmd = "Start", scenario = "No_Such_Scenario" }, host.TestTimeout);

        using var packet = await ReceiveJsonAsync(ws, host.TestTimeout);

        var events = packet.RootElement.GetProperty("e").EnumerateArray().ToArray();
        events.Should().HaveCountGreaterOrEqualTo(2);

        events.Select(e => e.GetProperty("type").GetString()).Should().Contain(new[] { "Error", "FlowStopped" });

        var error = events.Single(e => e.GetProperty("type").GetString() == "Error");
        error.GetProperty("payload").GetProperty("code").GetString().Should().Be("ERR_SCENARIO_NOT_FOUND");
        error.GetProperty("payload").GetProperty("source").GetString().Should().Be("No_Such_Scenario");

        var stopped = events.Single(e => e.GetProperty("type").GetString() == "FlowStopped");
        stopped.GetProperty("payload").GetProperty("reason").GetString().Should().Be("Error");
    }

    [Fact]
    public async Task WebSocket_start_then_stop_emits_flow_started_and_flow_stopped()
    {
        await using var host = await TestServerHost.StartAsync();
        using var ws = await host.ConnectWebSocketAsync();

        await SendJsonAsync(ws, new { cmd = "Start", scenario = ScenarioName }, host.TestTimeout);

        using var first = await ReceiveJsonAsync(ws, host.TestTimeout);
        var firstEvents = first.RootElement.GetProperty("e").EnumerateArray().ToArray();

        var started = firstEvents.Single(e => e.GetProperty("type").GetString() == "FlowStarted");
        started.GetProperty("payload").GetProperty("scenario").GetString().Should().Be(ScenarioName);
        started.GetProperty("payload").GetProperty("schemaVersion").GetString().Should().NotBeNullOrWhiteSpace();
        started.GetProperty("payload").GetProperty("tickBase").GetInt64().Should().BeGreaterThan(0);

        // m 首帧应为 snapshot（至少包含一些设备键）
        first.RootElement.TryGetProperty("m", out var m).Should().BeTrue();
        m.ValueKind.Should().Be(JsonValueKind.Object);
        m.EnumerateObject().Should().NotBeEmpty();

        // 结合 RotaryLiftAssemblyTests 的 Name("右侧...") 语义，尽量验证 step 很快会进入业务步骤
        var step = first.RootElement.GetProperty("step").GetString() ?? "";
        step.Should().NotBeNull();

        await SendJsonAsync(ws, new { cmd = "Stop" }, host.TestTimeout);

        using var stoppedPacket = await ReceiveUntilAsync(
            ws,
            p => TryGetFlowStoppedReason(p, out var reason) && reason == "UserStop",
            host.TestTimeout);

        TryGetFlowStoppedReason(stoppedPacket, out var stopReason).Should().BeTrue();
        stopReason.Should().Be("UserStop");
    }

    private static bool TryGetFlowStoppedReason(JsonDocument doc, out string? reason)
    {
        reason = null;

        if (!doc.RootElement.TryGetProperty("e", out var e) || e.ValueKind != JsonValueKind.Array)
            return false;

        foreach (var evt in e.EnumerateArray())
        {
            if (evt.TryGetProperty("type", out var type) && type.GetString() == "FlowStopped")
            {
                if (evt.TryGetProperty("payload", out var payload)
                    && payload.TryGetProperty("reason", out var r))
                {
                    reason = r.GetString();
                    return true;
                }
            }
        }

        return false;
    }

    private static async Task SendJsonAsync(ClientWebSocket ws, object value, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, cts.Token);
    }

    private static async Task<JsonDocument> ReceiveJsonAsync(ClientWebSocket ws, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var text = await ReceiveTextAsync(ws, cts.Token);
        return JsonDocument.Parse(text);
    }

    private static async Task<JsonDocument> ReceiveUntilAsync(
        ClientWebSocket ws,
        Func<JsonDocument, bool> predicate,
        TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        var seen = new List<string>();

        while (DateTimeOffset.UtcNow < deadline)
        {
            var remaining = deadline - DateTimeOffset.UtcNow;
            using var cts = new CancellationTokenSource(remaining);

            var text = await ReceiveTextAsync(ws, cts.Token);
            seen.Add(text);

            var doc = JsonDocument.Parse(text);
            if (predicate(doc)) return doc;

            doc.Dispose();
        }

        throw new TimeoutException($"Expected packet not received in {timeout}. Last frames: {Math.Min(seen.Count, 3)}");
    }

    private static async Task<string> ReceiveTextAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = new byte[16 * 1024];
        var sb = new StringBuilder();

        while (true)
        {
            var result = await ws.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException("WebSocket closed while waiting for message.");

            sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (result.EndOfMessage)
                return sb.ToString();
        }
    }

    private sealed class TestServerHost : IAsyncDisposable
    {
        public Uri BaseUri { get; }
        public TimeSpan TestTimeout { get; } = TimeSpan.FromSeconds(8);

        private readonly WebApplication _app;

        private TestServerHost(WebApplication app, Uri baseUri)
        {
            _app = app;
            BaseUri = baseUri;
        }

        public static async Task<TestServerHost> StartAsync()
        {
            var port = GetFreeTcpPort();
            var baseUri = new Uri($"http://127.0.0.1:{port}");

            var app = ServerApp.Build(Array.Empty<string>());
            app.Urls.Clear();
            app.Urls.Add(baseUri.ToString());

            await app.StartAsync();
            return new TestServerHost(app, baseUri);
        }

        public async Task<ClientWebSocket> ConnectWebSocketAsync()
        {
            var wsUri = new UriBuilder(BaseUri)
            {
                Scheme = "ws",
                Path = "/ws/telemetry",
                Query = ""
            }.Uri;

            var ws = new ClientWebSocket();
            using var cts = new CancellationTokenSource(TestTimeout);
            await ws.ConnectAsync(wsUri, cts.Token);
            return ws;
        }

        public async ValueTask DisposeAsync()
        {
            try { await _app.StopAsync(); } catch { }
            await _app.DisposeAsync();
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
