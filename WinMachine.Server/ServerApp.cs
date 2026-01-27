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

        var app = builder.Build();

        var scenarios = new ScenarioRegistry();

        app.MapGet("/api/machine/schema", (string name) =>
        {
            var schema = scenarios.BuildSchema(name);
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

            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            var handler = new TelemetryWebSocketHandler(scenarios);
            await handler.RunAsync(ws, context.RequestAborted);
        });

        return app;
    }
}
