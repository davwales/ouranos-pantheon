using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap;

public static class GetVolumeHeatmapEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/volume-heatmap", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(nameof(GetVolumeHeatmapInput.LookbackWeeks))
            )
            .WithTags("Plutus.Trades");
    }

    internal static async Task<IResult> Handle(
        Id<Market> marketId,
        IMessageBus bus,
        int lookbackWeeks = 4,
        CancellationToken ct = default
    )
    {
        var result = await bus.InvokeAsync<GetVolumeHeatmapResponse>(
            new GetVolumeHeatmapInput(marketId, lookbackWeeks),
            ct
        );
        return Results.Ok(result);
    }
}
