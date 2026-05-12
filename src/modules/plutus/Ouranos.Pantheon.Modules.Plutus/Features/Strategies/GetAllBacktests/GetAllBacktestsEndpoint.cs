using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests;

public static class GetAllBacktestsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/strategies/{strategyId}/backtests", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllBacktestsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetAllBacktestsResponse>>(input, ct));
    }
}
