using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation;

public static class UpdateConversationEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/conversations/{conversationId}", Handle)
            .WithTags("Hermes.Conversations");
    }

    private static async Task<IResult> Handle(
        Id<Conversation> conversationId,
        UpdateConversationInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var command = input with { ConversationId = conversationId };
        return Results.Ok(await bus.InvokeAsync<IdResponse<Conversation>>(command, ct));
    }
}
