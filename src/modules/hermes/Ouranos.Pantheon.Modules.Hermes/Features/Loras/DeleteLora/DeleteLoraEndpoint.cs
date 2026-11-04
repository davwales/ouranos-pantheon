using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Hermes.Shared.PublicMode;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora;

public static class DeleteLoraEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/loras/{loraId}", Handle)
            .WithTags("Hermes.Loras")
            .DisabledInPublicMode();
    }

    internal static async Task<IResult> Handle(
        Id<Lora> loraId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<DeleteLoraResponse>(new DeleteLoraInput(loraId), ct)
        );
    }
}
