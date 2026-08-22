using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition;

public static class CreatePositionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/positions", Handle).WithTags("Plutus.Positions");
    }

    internal static async Task<IResult> Handle(
        CreatePositionBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new CreatePositionInput(
            body.Side,
            body.MarketId,
            body.SymbolId,
            body.Cost,
            body.Quantity,
            body.StrategyId,
            body.Notes
        );

        var result = await bus.InvokeAsync<IdResponse<Position>>(input, ct);
        return Results.Created($"/api/plutus/positions/{result.Id}", result);
    }
}
