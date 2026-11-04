using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet;

public static class CreateToolSetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/tool-sets", Handle)
            .WithTags("Hermes.ToolSets")
            .DisabledInPublicMode();
    }

    internal static async Task<IResult> Handle(
        CreateToolSetInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CreateToolSetResponse>(input, ct);
        return Results.Created($"/api/hermes/tool-sets/{result.ToolSetId}", result);
    }
}
