using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations;

public static class GetAllConversationsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/conversations", Handle)
            .WithTags("Hermes.Conversations");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllConversationsInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllConversationsResponse>>(input, ct));
    }
}
