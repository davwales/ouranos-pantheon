using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;

public static class CreateMarketEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/markets", Handle)
            .WithTags("Plutus.Markets");
    }

    private static async Task<IResult> Handle(
        CreateMarketInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        var result = await dispatcher.Send(input, ct);
        return Results.Created($"/api/plutus/markets/{result.Id}", result);
    }
}
