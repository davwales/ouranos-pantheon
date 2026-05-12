using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait;

public static class CreateTraitEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hermes/traits", Handle).WithTags("Hermes.Traits");
    }

    internal static async Task<IResult> Handle(
        CreateTraitInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<CreateTraitResponse>(input, ct);
        return Results.Created($"/api/hermes/traits/{result.TraitId}", result);
    }
}
