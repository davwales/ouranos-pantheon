using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet;

public static class DeleteToolSetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/tool-sets/{toolSetId}", Handle).WithTags("Hermes.ToolSets");
    }

    internal static async Task<IResult> Handle(
        Id<ToolSet> toolSetId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<DeleteToolSetResponse>(new DeleteToolSetInput(toolSetId), ct)
        );
    }
}
