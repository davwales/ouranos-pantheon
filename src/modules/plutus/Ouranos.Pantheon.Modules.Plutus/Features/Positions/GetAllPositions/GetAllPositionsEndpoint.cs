using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions;

public static class GetAllPositionsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/positions", Handle)
            .WithTags("Plutus.Positions");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllPositionsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(input, ct));
    }
}
