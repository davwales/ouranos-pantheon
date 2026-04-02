using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;

public static class CreateConversationEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/conversations", Handle)
            .WithTags("Hermes.Conversations");
    }

    internal static async Task<IResult> Handle(
        CreateConversationInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CreateConversationResponse>(input, ct);
        return Results.Created($"/api/hermes/conversations/{result.Id}", result);
    }
}
