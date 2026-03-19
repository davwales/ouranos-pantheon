using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant;

public static class GetAssistantEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/assistants/{assistantId}", Handle)
            .WithTags("Hermes.Assistants");
    }

    private static async Task<IResult> Handle(
        Id<Assistant> assistantId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetAssistantResponse>(new GetAssistantInput(assistantId), ct);
        return Results.Ok(result);
    }
}
