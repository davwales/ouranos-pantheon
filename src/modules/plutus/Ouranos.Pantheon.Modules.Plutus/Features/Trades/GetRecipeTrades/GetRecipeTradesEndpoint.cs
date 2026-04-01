using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;

public static class GetRecipeTradesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/recipe-trades", Handle)
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
