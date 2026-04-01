using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;

public static class UpdateMarketEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/plutus/markets/{marketId}", Handle)
            .WithTags("Plutus.Markets");
    }

    internal static async Task<IResult> Handle(
        UpdateMarketInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<IdResponse<Market>>(input, ct));
    }
}
