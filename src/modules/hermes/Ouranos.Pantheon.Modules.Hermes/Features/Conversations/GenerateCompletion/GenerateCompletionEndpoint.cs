using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Shared.API;
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
        SseWriter.SetSseHeaders(httpContext.Response);

        var stream = await bus.InvokeAsync<IAsyncEnumerable<GenerateCompletionResponse>>(input, ct);
        await foreach (var chunk in stream.WithCancellation(ct))
        {
            await SseWriter.WriteEventAsync(httpContext.Response, chunk, ct);
        }
    }
}