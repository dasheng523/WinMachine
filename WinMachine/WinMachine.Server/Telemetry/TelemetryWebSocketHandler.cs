using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Reactive.Linq;
using Machine.Framework.Interpreters.Flow;
using Machine.Framework.Telemetry.Contracts;
using Machine.Framework.Telemetry.Runtime;
using WinMachine.Server.Scenarios;

namespace WinMachine.Server.Telemetry;

internal sealed class TelemetryWebSocketHandler
{
    private readonly ScenarioRegistry _scenarios;
    private readonly TimeSpan _samplingInterval = TimeSpan.FromMilliseconds(100); // 10Hz

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public TelemetryWebSocketHandler(ScenarioRegistry scenarios)
    {
        _scenarios = scenarios;
    }

    public async Task RunAsync(WebSocket ws, CancellationToken requestAborted)
    {
        var clock = new MonotonicUnixClock();
        var outgoing = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = true
        });

        var sendLoop = Task.Run(async () =>
        {
            try
            {
                while (await outgoing.Reader.WaitToReadAsync(requestAborted))
                {
                    while (outgoing.Reader.TryRead(out var msg))
                    {
                        var bytes = Encoding.UTF8.GetBytes(msg);
                        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, requestAborted);
                    }
                }
            }
            catch
            {
                // Ignore: connection is closing.
            }
        }, requestAborted);

        RunState? run = null;

        try
        {
            while (ws.State == WebSocketState.Open && !requestAborted.IsCancellationRequested)
            {
                var message = await ReceiveTextAsync(ws, requestAborted);
                if (message == null) break;

                ClientCommand? cmd;
                try
                {
                    cmd = JsonSerializer.Deserialize<ClientCommand>(message, JsonOptions);
                }
                catch
                {
                    continue;
                }

                if (cmd?.Cmd is null) continue;

                if (string.Equals(cmd.Cmd, "Start", StringComparison.OrdinalIgnoreCase))
                {
                    var scenarioName = cmd.Scenario ?? "";

                    if (run != null)
                    {
                        run.StopReason = FlowStopReason.UserStop;
                        run.Cts.Cancel();
                    }

                    var cts = new CancellationTokenSource();

                    ScenarioRuntime runtime;
                    try
                    {
                        runtime = _scenarios.BuildRuntime(scenarioName, cts.Token);
                    }
                    catch (Exception ex)
                    {
                        cts.Dispose();

                        var packet = new TelemetryPacket
                        {
                            Tick = clock.Now(),
                            Step = "",
                            Events = new()
                            {
                                TelemetryEvent.Error(ex.Message, code: "ERR_SCENARIO_NOT_FOUND", source: scenarioName),
                                TelemetryEvent.FlowStopped(FlowStopReason.Error)
                            }
                        };

                        outgoing.Writer.TryWrite(JsonSerializer.Serialize(packet, JsonOptions));
                        continue;
                    }

                    var interpreter = new SimulationFlowInterpreter();
                    interpreter.InitializeDevices(runtime.Context);

                    var session = new TelemetrySession(
                        runtime.Context,
                        interpreter.TraceStream,
                        interval: _samplingInterval);

                    session.ForceSnapshot();

                    var tickBase = clock.Now();
                    session.Enqueue(TelemetryEvent.FlowStarted(runtime.ScenarioName, tickBase, runtime.SchemaVersion));

                    var sub = session.Stream.Skip(1).Subscribe(packet =>
                    {
                        var json = JsonSerializer.Serialize(packet, JsonOptions);
                        outgoing.Writer.TryWrite(json);
                    });

                    run = new RunState(cts, session, sub) { StopReason = FlowStopReason.UserStop };

                    run.RunTask = Task.Run(async () =>
                    {
                        try
                        {
                            await interpreter.RunAsync(runtime.Flow, runtime.Context);
                            session.Enqueue(TelemetryEvent.FlowStopped(FlowStopReason.Complete));
                        }
                        catch (OperationCanceledException)
                        {
                            session.Enqueue(TelemetryEvent.FlowStopped(run.StopReason));
                        }
                        catch (Exception ex)
                        {
                            session.Enqueue(TelemetryEvent.Error(ex.Message));
                            session.Enqueue(TelemetryEvent.FlowStopped(FlowStopReason.Error));
                        }
                    }, CancellationToken.None);

                    continue;
                }

                if (string.Equals(cmd.Cmd, "Stop", StringComparison.OrdinalIgnoreCase))
                {
                    if (run == null)
                    {
                        var packet = new TelemetryPacket
                        {
                            Tick = clock.Now(),
                            Step = "",
                            Events = new() { TelemetryEvent.FlowStopped(FlowStopReason.UserStop) }
                        };

                        outgoing.Writer.TryWrite(JsonSerializer.Serialize(packet, JsonOptions));
                        continue;
                    }

                    run.StopReason = FlowStopReason.UserStop;
                    run.Cts.Cancel();
                    continue;
                }
            }
        }
        finally
        {
            if (run != null)
            {
                run.StopReason = FlowStopReason.UserStop;
                run.Cts.Cancel();
                try { if (run.RunTask != null) await run.RunTask; } catch { }

                run.Dispose();
            }

            outgoing.Writer.TryComplete();
            try { await sendLoop; } catch { }
        }
    }

    private static async Task<string?> ReceiveTextAsync(WebSocket ws, CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);

        try
        {
            var sb = new StringBuilder();

            while (true)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await ws.ReceiveAsync(buffer, ct);
                }
                catch (WebSocketException)
                {
                    return null;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                if (result.EndOfMessage)
                {
                    return sb.ToString();
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private sealed class RunState : IDisposable
    {
        public CancellationTokenSource Cts { get; }
        public TelemetrySession Session { get; }
        public IDisposable Subscription { get; }
        public Task? RunTask { get; set; }
        public FlowStopReason StopReason { get; set; }

        public RunState(CancellationTokenSource cts, TelemetrySession session, IDisposable subscription)
        {
            Cts = cts;
            Session = session;
            Subscription = subscription;
        }

        public void Dispose()
        {
            Subscription.Dispose();
            Session.Dispose();
            Cts.Dispose();
        }
    }

    private sealed class ClientCommand
    {
        [JsonPropertyName("cmd")]
        public string? Cmd { get; set; }

        [JsonPropertyName("scenario")]
        public string? Scenario { get; set; }
    }
}
