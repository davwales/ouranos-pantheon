using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora;

public static class UpdateLoraEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/loras/{loraId}", Handle).WithTags("Hermes.Loras");
    }

    internal static async Task<IResult> Handle(
        Id<Lora> loraId,
        UpdateLoraBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<UpdateLoraResponse>(
                new UpdateLoraInput(loraId, body.Name, body.Identifier, body.Weight, body.IsPublic),
                ct
            )
        );
    }
}
