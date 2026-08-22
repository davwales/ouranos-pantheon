using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview;

public static class GetMarketOverviewEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/overview", Handle).WithTags("Plutus.Trades");
    }

    internal static async Task<IResult> Handle(
        Id<Market> marketId,
        IMessageBus bus,
        TimeFrame timeFrame = TimeFrame.OneHour,
        int numBuckets = 100,
        CancellationToken ct = default
    )
    {
        var result = await bus.InvokeAsync<GetMarketOverviewResponse>(
            new GetMarketOverviewInput(marketId, timeFrame, numBuckets),
            ct
        );
        return Results.Ok(result);
    }
}
