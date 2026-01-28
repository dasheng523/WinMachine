using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Machine.Framework.Telemetry.Contracts;

public sealed class TelemetryPacket
{
    [JsonPropertyName("t")]
    public long Tick { get; set; }

    [JsonPropertyName("step")]
    public string Step { get; set; } = "";

    [JsonPropertyName("m")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, double>? Motions { get; set; }

    [JsonPropertyName("io")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Io { get; set; }

    [JsonPropertyName("mat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, MaterialInfo>? Materials { get; set; }

    [JsonPropertyName("e")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<TelemetryEvent>? Events { get; set; }
}

public sealed class TelemetryEvent
{
    [JsonPropertyName("type")]
    public EventType Type { get; set; }

    [JsonPropertyName("msg")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonPropertyName("payload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Payload { get; set; }

    public static TelemetryEvent FlowStarted(string scenario, long tickBase, string schemaVersion) =>
        new()
        {
            Type = EventType.FlowStarted,
            Payload = new FlowStartedPayload { Scenario = scenario, TickBase = tickBase, SchemaVersion = schemaVersion }
        };

    public static TelemetryEvent FlowStopped(FlowStopReason reason) =>
        new()
        {
            Type = EventType.FlowStopped,
            Payload = new FlowStoppedPayload { Reason = reason }
        };

    public static TelemetryEvent Error(string message, string? code = null, string? source = null) =>
        new()
        {
            Type = EventType.Error,
            Message = message,
            Payload = new ErrorPayload { Code = code, Source = source }
        };

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventType
{
    FlowStarted,
    FlowStopped,
    Error,
    Attach,
    Detach,
    Spawn,
    MaterialSpawn,
    MaterialTransform,
    MaterialConsume
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FlowStopReason
{
    Complete,
    Error,
    UserStop
}

public sealed class FlowStartedPayload
{
    [JsonPropertyName("scenario")]
    public string Scenario { get; set; } = "";

    [JsonPropertyName("tickBase")]
    public long TickBase { get; set; }

    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "";
}

public sealed class FlowStoppedPayload
{
    [JsonPropertyName("reason")]
    public FlowStopReason Reason { get; set; }
}

public sealed class ErrorPayload
{
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }
}

public sealed class MaterialInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("class")]
    public string Class { get; set; } = "";
}

public sealed class MaterialEventPayload
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AtStation { get; set; }

    [JsonPropertyName("class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Class { get; set; }

    [JsonPropertyName("to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToClass { get; set; }

    [JsonPropertyName("parent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ParentId { get; set; }

    [JsonPropertyName("child")] // Used for Attach child ID
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ChildId { get; set; }
}
