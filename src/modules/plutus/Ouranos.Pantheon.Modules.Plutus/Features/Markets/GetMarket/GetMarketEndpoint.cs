using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket;

public static class GetMarketEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}", Handle)
            .WithTags("Plutus.Markets");
    }

    private static async Task<IResult> Handle(
        Id<Market> marketId,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        var result = await dispatcher.Send(new GetMarketInput(marketId), ct);
        return Results.Ok(result);
    }
}
