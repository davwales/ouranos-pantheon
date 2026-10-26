using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests;

public static class GetActiveBacktestsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/backtests/active", Handle).WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetActiveBacktestsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<PagedResponse<GetActiveBacktestsResponse>>(input, ct)
        );
    }
}
