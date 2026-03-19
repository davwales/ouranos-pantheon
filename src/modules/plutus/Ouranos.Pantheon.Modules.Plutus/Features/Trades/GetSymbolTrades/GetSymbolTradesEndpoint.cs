using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;

public static class GetSymbolTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/trades", Handle)
            .WithTags("Plutus.Trades");
    }

    private static async Task<IResult> Handle(
        Id<Symbol> symbolId,
        IScopedDispatcher dispatcher,
        double? seconds,
        int numBuckets = 100,
        CancellationToken ct = default
    )
    {
        var result = await dispatcher.Send(new GetSymbolTradesInput(symbolId, numBuckets, seconds), ct);
        return Results.Ok(result);
    }
}
