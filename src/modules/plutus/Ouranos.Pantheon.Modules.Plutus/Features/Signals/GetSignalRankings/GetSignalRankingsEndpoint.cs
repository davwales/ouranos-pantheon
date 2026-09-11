using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;

public static class GetSignalRankingsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/markets/{marketId}/signal-rankings", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(
                        nameof(GetSignalRankingsInput.SortField),
                        nameof(GetSignalRankingsInput.SortDirection),
                        nameof(GetSignalRankingsInput.Skip),
                        nameof(GetSignalRankingsInput.Take),
                        nameof(GetSignalRankingsInput.Filter)
                    )
            )
            .WithTags("Plutus.Signals");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetSignalRankingsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(input, ct)
        );
    }
}
