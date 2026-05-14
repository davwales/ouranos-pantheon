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

    internal static async Task<IResult> Handle(
        Id<Conversation> conversationId,
        UpdateConversationBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdateConversationInput(
            conversationId,
            body.Name,
            body.PersonaId,
            body.ModelConfigId,
            body.TraitIds,
            body.Messages,
            body.IsPublic
        );
        return Results.Ok(await bus.InvokeAsync<IdResponse<Conversation>>(input, ct));
    }
}
