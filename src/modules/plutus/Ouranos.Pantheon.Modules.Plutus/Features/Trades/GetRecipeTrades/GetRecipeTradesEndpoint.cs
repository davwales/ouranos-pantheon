using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;

public static class GetRecipeTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/recipe-trades", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(
                        nameof(GetRecipeTradesInput.TimeFrame),
                        nameof(GetRecipeTradesInput.SortField),
                        nameof(GetRecipeTradesInput.SortDirection),
                        nameof(GetRecipeTradesInput.Skip),
                        nameof(GetRecipeTradesInput.Take),
                        nameof(GetRecipeTradesInput.Filter)
                    )
            )
            .WithTags("Plutus.Trades");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetRecipeTradesInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetRecipeTradesResponse>>(input, ct));
    }
}
