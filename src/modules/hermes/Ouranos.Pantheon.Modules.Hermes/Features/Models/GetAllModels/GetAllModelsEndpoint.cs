using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels;

public static class GetAllModelsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/models", Handle).WithTags("Hermes.Models");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllModelsInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllModelsResponse>>(input, ct));
    }
}
