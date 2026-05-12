using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;
using Ouranos.Pantheon.Modules.Shared.API;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation;

public static class CompactConversationEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/conversations/compact", Handle).WithTags("Hermes.Conversations");
    }

    internal static async Task Handle(
        CompactConversationInput input,
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        SseWriter.SetSseHeaders(httpContext.Response);

        var stream = await bus.InvokeAsync<IAsyncEnumerable<CompactConversationResponse>>(
            input,
            ct
        );
        await foreach (var chunk in stream.WithCancellation(ct))
        {
            await SseWriter.WriteEventAsync(httpContext.Response, chunk, ct);
        }
    }
}
