using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;

public static class UpdateTraitEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hermes/traits/{traitId}", Handle)
            .WithTags("Hermes.Traits");
    }

    internal static async Task<IResult> Handle(
        UpdateTraitInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<UpdateTraitResponse>(input, ct));
    }
}
