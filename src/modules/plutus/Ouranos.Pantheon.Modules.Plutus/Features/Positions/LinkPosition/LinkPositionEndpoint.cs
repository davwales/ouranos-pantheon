using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition;

public static class LinkPositionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/positions/{positionId}/link", Handle).WithTags("Plutus.Positions");
    }

    internal static async Task<IResult> Handle(
        Id<Position> positionId,
        LinkPositionBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new LinkPositionInput(positionId, body.TargetPositionId);
        return Results.Ok(await bus.InvokeAsync<IdResponse<Position>>(input, ct));
    }
}
