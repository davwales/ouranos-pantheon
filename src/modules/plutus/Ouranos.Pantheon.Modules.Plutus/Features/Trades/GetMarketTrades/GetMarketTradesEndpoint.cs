using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;

public static class GetMarketTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/trades", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(
                        nameof(GetMarketTradesInput.TimeFrame),
                        nameof(GetMarketTradesInput.SortField),
                        nameof(GetMarketTradesInput.SortDirection),
                        nameof(GetMarketTradesInput.Skip),
                        nameof(GetMarketTradesInput.Take),
                        nameof(GetMarketTradesInput.Filter)
                    )
            )
            .WithTags("Plutus.Trades");
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
