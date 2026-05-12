using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;

public static class DeleteMarketEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/plutus/markets/{marketId}", Handle).WithTags("Plutus.Markets");
    }

    internal static async Task<IResult> Handle(
        Id<Market> marketId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<IdResponse<Market>>(new DeleteMarketInput(marketId), ct)
        );
    }
}
