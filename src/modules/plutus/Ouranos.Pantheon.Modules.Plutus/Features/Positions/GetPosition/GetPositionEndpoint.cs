using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition;

public static class GetPositionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/positions/{positionId}", Handle).WithTags("Plutus.Positions");
    }

    internal static async Task<IResult> Handle(
        Id<Position> positionId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<GetPositionResponse>(new GetPositionInput(positionId), ct)
        );
    }
}
