using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet;

public static class UpdateToolSetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/tool-sets/{toolSetId}", Handle)
            .WithTags("Hermes.ToolSets")
            .DisabledInPublicMode();
    }

    internal static async Task<IResult> Handle(
        Id<ToolSet> toolSetId,
        UpdateToolSetBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<UpdateToolSetResponse>(
                new UpdateToolSetInput(
                    toolSetId,
                    body.Name,
                    body.Description,
                    body.Instructions,
                    body.Tools,
                    body.IsPublic
                ),
                ct
            )
        );
    }
}
