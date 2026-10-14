using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy;

public static class DeleteStrategyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/plutus/strategies/{strategyId}", Handle).WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        IMessageBus bus,
        IOutputCacheStore outputCacheStore,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Strategy>>(
            new DeleteStrategyInput(strategyId),
            ct
        );

        await outputCacheStore.EvictByTagAsync(RecommendationCache.TagFor(strategyId), ct);
        return Results.Ok(result);
    }
}
