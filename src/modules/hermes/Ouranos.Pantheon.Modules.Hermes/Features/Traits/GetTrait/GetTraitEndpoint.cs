using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;

public static class GetTraitEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hermes/traits/{traitId}", Handle)
            .WithTags("Hermes.Traits");
    }

    internal static async Task<IResult> Handle(
        Id<Trait> traitId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetTraitResponse>(new GetTraitInput(traitId), ct);
        return Results.Ok(result);
    }
}
