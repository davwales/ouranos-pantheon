using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets;

public static class GetAllToolSetsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/tool-sets", Handle).WithTags("Hermes.ToolSets");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllToolSetsInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllToolSetsResponse>>(input, ct));
    }
}
