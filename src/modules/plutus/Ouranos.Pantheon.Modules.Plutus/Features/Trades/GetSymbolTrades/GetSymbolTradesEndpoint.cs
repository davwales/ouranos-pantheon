using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;

public static class GetSymbolTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/trades", Handle)
            .WithTags("Plutus.Trades");
    }

    internal static async Task<IResult> Handle(
        Id<Symbol> symbolId,
        IMessageBus bus,
        TimeFrame timeFrame = TimeFrame.OneHour,
        int numBuckets = 100,
        CancellationToken ct = default
    )
    {
        var result = await bus.InvokeAsync<GetSymbolTradesResponse>(
            new GetSymbolTradesInput(symbolId, timeFrame, numBuckets),
            ct
        );
        return Results.Ok(result);
    }
}
