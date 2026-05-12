using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition;

public static class UpdatePositionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/plutus/positions/{positionId}", Handle).WithTags("Plutus.Positions");
    }

    internal static async Task<IResult> Handle(
        Id<Position> positionId,
        UpdatePositionBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdatePositionInput(positionId, body.Cost, body.Quantity, body.Notes);
        return Results.Ok(await bus.InvokeAsync<IdResponse<Position>>(input, ct));
    }
}
