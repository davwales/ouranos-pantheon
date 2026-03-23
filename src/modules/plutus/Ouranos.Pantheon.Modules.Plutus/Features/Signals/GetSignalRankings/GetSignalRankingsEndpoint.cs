using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;

public static class GetSignalRankingsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/signal-rankings", Handle)
            .WithTags("Plutus.Signals");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetSignalRankingsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(input, ct));
    }
}
