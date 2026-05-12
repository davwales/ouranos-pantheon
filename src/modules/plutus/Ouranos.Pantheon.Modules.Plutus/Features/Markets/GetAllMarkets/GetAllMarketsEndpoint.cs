using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;

public static class GetAllMarketsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets", Handle).WithTags("Plutus.Markets");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllMarketsInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<List<GetAllMarketsResponse>>(input, ct));
    }
}
