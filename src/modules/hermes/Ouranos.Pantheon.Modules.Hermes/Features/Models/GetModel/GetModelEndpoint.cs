using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;

public static class GetModelEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/models/{modelId}", Handle)
            .WithTags("Hermes.Models");
    }

    private static async Task<IResult> Handle(
        Id<ModelConfig> modelId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetModelResponse>(new GetModelInput(modelId), ct);
        return Results.Ok(result);
    }
}
