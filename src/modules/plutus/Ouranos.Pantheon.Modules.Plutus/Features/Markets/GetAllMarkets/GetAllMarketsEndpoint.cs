using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;

public static class GetAllMarketsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets", Handle)
            .WithTags("Plutus.Markets");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllMarketsInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        var wrapper = await dispatcher.Send(input, ct);
        return Results.Ok(wrapper.Value.ToList());
    }
}
