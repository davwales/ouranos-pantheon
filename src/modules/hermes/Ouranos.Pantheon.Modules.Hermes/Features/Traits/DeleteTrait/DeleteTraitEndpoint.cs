using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;

public static class DeleteTraitEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/hermes/traits/{traitId}", Handle)
            .WithTags("Hermes.Traits");
    }

    internal static async Task<IResult> Handle(
        Id<Trait> traitId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<DeleteTraitResponse>(new DeleteTraitInput(traitId), ct));
    }
}
