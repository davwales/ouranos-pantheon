using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora;

public static class GetLoraEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/loras/{loraId}", Handle).WithTags("Hermes.Loras");
    }

    internal static async Task<IResult> Handle(
        Id<Lora> loraId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetLoraResponse>(new GetLoraInput(loraId), ct);
        return Results.Ok(result);
    }
}
