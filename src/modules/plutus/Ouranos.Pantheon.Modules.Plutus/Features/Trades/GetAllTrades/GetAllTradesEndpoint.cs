using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Wolverine;

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
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllTradesResponse>>(input, ct));
    }
}
