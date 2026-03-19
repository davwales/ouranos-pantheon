using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants.Schemas;
using Wolverine;

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
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllAssistantsResponse>>(input, ct));
    }
}
