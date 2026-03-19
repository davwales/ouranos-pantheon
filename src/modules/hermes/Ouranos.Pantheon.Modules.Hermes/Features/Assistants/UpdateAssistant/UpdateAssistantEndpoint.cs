using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant;

public static class UpdateAssistantEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/assistants/{assistantId}", Handle)
            .WithTags("Hermes.Assistants");
    }

    private static async Task<IResult> Handle(
        UpdateAssistantInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}
