using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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
        app.MapGet("/api/plutus/markets/{marketId}/overview", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(nameof(GetMarketOverviewInput.TimeFrame))
            )
            .WithTags("Plutus.Trades");
    }

    internal static async Task<IResult> Handle(
        Id<Market> marketId,
        IMessageBus bus,
        TimeFrame timeFrame = TimeFrame.OneHour,
        CancellationToken ct = default
    )
    {
        var result = await bus.InvokeAsync<GetMarketOverviewResponse>(
            new GetMarketOverviewInput(marketId, timeFrame),
            ct
        );
        return Results.Ok(result);
    }
}
