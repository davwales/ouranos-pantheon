using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition;

public static class ClosePositionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/positions/{positionId}/close", Handle)
            .WithTags("Plutus.Positions");
    }

    internal static async Task<IResult> Handle(
        Id<Position> positionId,
        ClosePositionBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new ClosePositionInput(positionId, body.CloseStatus);
        return Results.Ok(await bus.InvokeAsync<ClosePositionResponse>(input, ct));
    }
}
