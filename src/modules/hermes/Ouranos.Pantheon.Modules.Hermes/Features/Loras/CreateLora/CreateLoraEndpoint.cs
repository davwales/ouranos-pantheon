using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora;

public static class CreateLoraEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/loras", Handle).WithTags("Hermes.Loras");
    }

    internal static async Task<IResult> Handle(
        CreateLoraInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CreateLoraResponse>(input, ct);
        return Results.Created($"/api/hermes/loras/{result.LoraId}", result);
    }
}
