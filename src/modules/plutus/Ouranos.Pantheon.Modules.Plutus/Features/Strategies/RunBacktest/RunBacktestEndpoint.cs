using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public static class RunBacktestEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/strategies/{strategyId}/backtest", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        RunBacktestBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new RunBacktestInput(
            strategyId,
            body.MarketId,
            body.StartDate,
            body.EndDate,
            body.Budget,
            body.VolumeParticipationRate ?? 0.25m,
            body.SlippageMultiplier ?? 0.1m
        );
        var result = await bus.InvokeAsync<RunBacktestResponse>(input, ct);
        return Results.Accepted($"/api/plutus/backtests/{result.BacktestId}", result);
    }
}
