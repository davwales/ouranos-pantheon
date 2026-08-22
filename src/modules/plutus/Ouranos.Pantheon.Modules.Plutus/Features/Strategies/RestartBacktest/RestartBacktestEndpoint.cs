using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest;

public static class RestartBacktestEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/backtests/{backtestId}/restart", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Backtest> backtestId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new RestartBacktestInput(backtestId);
        var result = await bus.InvokeAsync<RestartBacktestResponse>(input, ct);
        return Results.Accepted($"/api/plutus/backtests/{result.BacktestId}", result);
    }
}
