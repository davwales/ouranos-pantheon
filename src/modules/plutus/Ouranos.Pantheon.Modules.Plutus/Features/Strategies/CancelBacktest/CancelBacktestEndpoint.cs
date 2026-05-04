using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest;

public static class CancelBacktestEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/backtests/{backtestId}/cancel", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Backtest> backtestId,
        CancelBacktestBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new CancelBacktestInput(backtestId, body.Reason);
        var result = await bus.InvokeAsync<CancelBacktestResponse>(input, ct);
        return Results.Ok(result);
    }
}