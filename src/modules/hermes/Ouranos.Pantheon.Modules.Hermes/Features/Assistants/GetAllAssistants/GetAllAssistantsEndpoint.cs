using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants;

public static class GetAllAssistantsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/assistants", Handle)
            .WithTags("Hermes.Assistants");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllAssistantsInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        var wrapper = await dispatcher.Send(input, ct);
        return Results.Ok(wrapper.Value.ToList());
    }
}
