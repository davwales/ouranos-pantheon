using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest;

public static class GetBacktestEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/backtests/{backtestId}", Handle).WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Backtest> backtestId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<GetBacktestResponse>(new GetBacktestInput(backtestId), ct)
        );
    }
}
