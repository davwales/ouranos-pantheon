using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;

public static class UpdateMarketEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/plutus/markets/{marketId}", Handle)
            .WithTags("Plutus.Markets");
    }

    private static async Task<IResult> Handle(
        UpdateMarketInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}
