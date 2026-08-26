using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;

public static class UpdateTraitEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/traits/{traitId}", Handle).WithTags("Hermes.Traits");
    }

    internal static async Task<IResult> Handle(
        Id<Trait> traitId,
        UpdateTraitBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdateTraitInput(traitId, body.Name, body.Content, body.IsPublic);
        return Results.Ok(await bus.InvokeAsync<UpdateTraitResponse>(input, ct));
    }
}
