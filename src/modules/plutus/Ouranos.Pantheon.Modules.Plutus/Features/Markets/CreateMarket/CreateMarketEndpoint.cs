using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;

public static class CreateMarketEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/markets", Handle)
            .WithTags("Plutus.Markets");
    }

    internal static async Task<IResult> Handle(
        CreateMarketInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Market>>(input, ct);
        return Results.Created($"/api/plutus/markets/{result.Id}", result);
    }
}
