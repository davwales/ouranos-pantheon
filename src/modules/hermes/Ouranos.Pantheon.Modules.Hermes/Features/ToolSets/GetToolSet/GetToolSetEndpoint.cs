using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet;

public static class GetToolSetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/tool-sets/{toolSetId}", Handle).WithTags("Hermes.ToolSets");
    }

    internal static async Task<IResult> Handle(
        Id<ToolSet> toolSetId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetToolSetResponse>(new GetToolSetInput(toolSetId), ct);
        return Results.Ok(result);
    }
}
