using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;
using Ouranos.Pantheon.Modules.Shared.Contract.API;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation;

public static class CompactConversationEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/conversations/compact", Handle)
            .WithTags("Hermes.Conversations")
            .DisabledInPublicMode<CompactConversationInput>(input =>
                input.ConversationId is not null
            );
    }

    internal static async Task Handle(
        CompactConversationInput input,
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        var stream = await bus.InvokeAsync<IAsyncEnumerable<CompactConversationResponse>>(
            input,
            ct
        );
        await SseWriter.WriteStreamAsync(httpContext.Response, stream, ct);
    }
}
