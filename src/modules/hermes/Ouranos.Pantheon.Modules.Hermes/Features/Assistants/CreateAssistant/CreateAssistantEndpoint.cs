using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant;

public static class CreateAssistantEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/assistants", Handle)
            .WithTags("Hermes.Assistants");
    }

    private static async Task<IResult> Handle(
        CreateAssistantInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        var result = await dispatcher.Send(input, ct);
        return Results.Created($"/api/hermes/assistants/{result.Id}", result);
    }
}
