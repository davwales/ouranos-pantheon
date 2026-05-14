using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels;

public static class GetAvailableModelsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/available-models", Handle).WithTags("Hermes.Models");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAvailableModelsInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAvailableModelsResponse>>(input, ct));
    }
}
