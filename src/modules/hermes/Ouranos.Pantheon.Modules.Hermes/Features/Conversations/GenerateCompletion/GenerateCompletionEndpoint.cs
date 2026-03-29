using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Wolverine;

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
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        httpContext.Response.Headers.ContentType = "text/event-stream";
        httpContext.Response.Headers.CacheControl = "no-cache";
        httpContext.Response.Headers.Connection = "keep-alive";

        var stream = await bus.InvokeAsync<IAsyncEnumerable<GenerateCompletionResponse>>(input, ct);
        await foreach (var chunk in stream.WithCancellation(ct))
        {
            var json = JsonSerializer.Serialize(chunk, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            await httpContext.Response.WriteAsync($"data: {json}\n\n", ct);
            await httpContext.Response.Body.FlushAsync(ct);
        }
    }
}
