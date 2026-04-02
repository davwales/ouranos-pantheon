using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel;

public static class DeleteModelEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/models/{modelId}", Handle)
            .WithTags("Hermes.Models");
    }

    internal static async Task<IResult> Handle(
        Id<ModelConfig> modelId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<DeleteModelResponse>(new DeleteModelInput(modelId), ct));
    }
}
