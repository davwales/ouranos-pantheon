using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;

public static class GetAllTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/trades", Handle)
            .WithTags("Plutus.Trades");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllTradesInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct = default
    )
    {
        var wrapper = await dispatcher.Send(input, ct);
        return Results.Ok(wrapper.Value);
    }
}
