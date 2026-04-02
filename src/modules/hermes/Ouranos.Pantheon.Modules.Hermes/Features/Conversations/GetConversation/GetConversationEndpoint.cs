using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation;

public static class GetConversationEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/conversations/{conversationId}", Handle)
            .WithTags("Hermes.Conversations");
    }

    internal static async Task<IResult> Handle(
        Id<Conversation> conversationId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<GetConversationResponse>(new GetConversationInput(conversationId), ct));
    }
}
