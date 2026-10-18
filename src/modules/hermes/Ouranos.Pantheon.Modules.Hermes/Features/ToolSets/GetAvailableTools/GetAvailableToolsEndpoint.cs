using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools;

public static class GetAvailableToolsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/tools", Handle).WithTags("Hermes.ToolSets");
    }

    internal static async Task<IResult> Handle(IMessageBus bus, CancellationToken ct)
    {
        return Results.Ok(
            await bus.InvokeAsync<List<GetAvailableToolsResponse>>(new GetAvailableToolsInput(), ct)
        );
    }
}
