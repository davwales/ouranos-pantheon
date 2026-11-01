using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel;

public static class CreateModelEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/models", Handle).WithTags("Hermes.Models").DisabledInPublicMode();
    }

    internal static async Task<IResult> Handle(
        CreateModelInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CreateModelResponse>(input, ct);
        return Results.Created($"/api/hermes/models/{result.ModelId}", result);
    }
}
