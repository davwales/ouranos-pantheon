using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;

public static class GetMarketTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/trades", Handle)
            .WithTags("Plutus.Trades");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetMarketTradesInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}
