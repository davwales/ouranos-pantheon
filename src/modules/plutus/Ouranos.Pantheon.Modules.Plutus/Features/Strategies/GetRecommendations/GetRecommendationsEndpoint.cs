using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;

public static class GetRecommendationsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/strategies/{strategyId}/recommendations", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(RecommendationCache.Duration)
                    .SetVaryByQuery(
                        nameof(GetRecommendationsInput.MarketId),
                        nameof(GetRecommendationsInput.Budget),
                        nameof(GetRecommendationsInput.VolumeParticipationRate)
                    )
            )
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetRecommendationsInput input,
        IMessageBus bus,
        HttpContext httpContext,
        CancellationToken ct = default
    )
    {
        httpContext
            .Features.Get<IOutputCacheFeature>()
            ?.Context.Tags.Add(RecommendationCache.TagFor(input.StrategyId));

        return Results.Ok(await bus.InvokeAsync<GetRecommendationsResponse>(input, ct));
    }
}
