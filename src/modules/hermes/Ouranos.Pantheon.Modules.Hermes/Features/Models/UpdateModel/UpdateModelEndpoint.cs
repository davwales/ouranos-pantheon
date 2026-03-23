using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;

public static class UpdateModelEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/models/{modelId}", Handle)
            .WithTags("Hermes.Models");
    }

    private static async Task<IResult> Handle(
        UpdateModelInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<UpdateModelResponse>(input, ct));
    }
}
