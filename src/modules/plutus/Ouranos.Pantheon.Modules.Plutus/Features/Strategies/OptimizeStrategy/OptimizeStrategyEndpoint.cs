using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;

public static class OptimizeStrategyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/strategies/{strategyId}/optimize", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        OptimizeStrategyBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new OptimizeStrategyInput(
            strategyId,
            body.MarketId,
            body.StartDate,
            body.EndDate,
            body.Budget,
            body.Generations,
            body.PopulationSize,
            body.SharpeRatioWeight,
            body.TotalReturnWeight,
            body.MaxDrawdownWeight,
            body.VolumeParticipationRate ?? 0.25m,
            body.SlippageMultiplier ?? 0.1m
        );
        var result = await bus.InvokeAsync<OptimizeStrategyResponse>(input, ct);
        return Results.Accepted($"/api/plutus/backtests/{result.BacktestId}", result);
    }
}
