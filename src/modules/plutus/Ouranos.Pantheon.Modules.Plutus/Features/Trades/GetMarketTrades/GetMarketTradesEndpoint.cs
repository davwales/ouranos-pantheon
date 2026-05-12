using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;

public static class GetMarketTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/trades", Handle).WithTags("Plutus.Trades");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetMarketTradesInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(input, ct));
    }
}
