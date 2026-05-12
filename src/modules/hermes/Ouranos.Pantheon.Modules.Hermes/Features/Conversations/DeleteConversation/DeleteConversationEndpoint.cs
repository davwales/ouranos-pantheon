using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation;

public static class DeleteConversationEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/conversations/{conversationId}", Handle)
            .WithTags("Hermes.Conversations");
    }

    internal static async Task<IResult> Handle(
        Id<Conversation> conversationId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<IdResponse<Conversation>>(
                new DeleteConversationInput(conversationId),
                ct
            )
        );
    }
}
