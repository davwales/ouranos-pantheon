using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;

public static class GetRecommendationsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/strategies/{strategyId}/recommendations", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        GetRecommendationsBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new GetRecommendationsInput(strategyId, body.MarketId, body.Budget);
        return Results.Ok(await bus.InvokeAsync<GetRecommendationsResponse>(input, ct));
    }
}