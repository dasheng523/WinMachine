using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WinMachine.Server.Scenarios;
using WinMachine.Server.Telemetry;

namespace WinMachine.Server;

public static class ServerApp
{
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddCors();

        var app = builder.Build();

        app.UseCors(policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());

        var scenarios = new ScenarioRegistry();

        app.MapGet("/api/machine/scenarios", () =>
        {
            var list = scenarios.Names.OrderBy(x => x).ToArray();
            return Results.Json(list);
        });

        app.MapGet("/api/machine/schema", (string? name) =>
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Results.BadRequest(new ErrorResponse(
                    Code: "ERR_SCENARIO_REQUIRED",
                    Message: "Query parameter 'name' is required.",
                    KnownScenarios: scenarios.Names.OrderBy(x => x).ToArray()));
            }

            if (!scenarios.TryGet(name, out var scenario) || scenario is null)
            {
                return Results.NotFound(new ErrorResponse(
                    Code: "ERR_SCENARIO_NOT_FOUND",
                    Message: $"Unknown scenario '{name}'.",
                    KnownScenarios: scenarios.Names.OrderBy(x => x).ToArray()));
            }

            var schema = scenario.BuildSchema();
            return Results.Json(schema);
        });

        app.UseWebSockets();

        app.Map("/ws/telemetry", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            Console.WriteLine($"[WS] Incoming request from {context.Connection.RemoteIpAddress}");

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            Console.WriteLine("[WS] Connection accepted.");

            var handler = new TelemetryWebSocketHandler(scenarios);
            try
            {
                await handler.RunAsync(ws, context.RequestAborted);
            }
            finally
            {
                Console.WriteLine("[WS] Connection closed.");
            }
        });

        return app;
    }

    private sealed record ErrorResponse(string Code, string Message, string[] KnownScenarios);
}
