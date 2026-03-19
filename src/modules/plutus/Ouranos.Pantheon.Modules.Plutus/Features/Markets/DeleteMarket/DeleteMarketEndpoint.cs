using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;

public static class DeleteMarketEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/plutus/markets/{marketId}", Handle)
            .WithTags("Plutus.Markets");
    }

    private static async Task<IResult> Handle(
        Id<Market> marketId,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        return Results.Ok(await dispatcher.Send(new DeleteMarketInput(marketId), ct));
    }
}
