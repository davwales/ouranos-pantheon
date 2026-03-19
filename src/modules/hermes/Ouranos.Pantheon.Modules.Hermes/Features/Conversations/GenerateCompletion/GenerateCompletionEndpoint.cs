using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

public static class GenerateCompletionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/conversations/completions/stream", Handle)
            .WithTags("Hermes.Conversations");
    }

    private static async Task Handle(
        GenerateCompletionInput input,
        IScopedDispatcher dispatcher,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        httpContext.Response.Headers.ContentType = "text/event-stream";
        httpContext.Response.Headers.CacheControl = "no-cache";
        httpContext.Response.Headers.Connection = "keep-alive";

        await foreach (var chunk in dispatcher.CreateStream(input, ct))
        {
            var json = JsonSerializer.Serialize(chunk, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            await httpContext.Response.WriteAsync($"data: {json}\n\n", ct);
            await httpContext.Response.Body.FlushAsync(ct);
        }
    }
}
