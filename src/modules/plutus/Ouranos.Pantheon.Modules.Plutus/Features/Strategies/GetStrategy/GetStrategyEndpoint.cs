using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy;

public static class GetStrategyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/strategies/{strategyId}", Handle).WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<GetStrategyResponse>(new GetStrategyInput(strategyId), ct)
        );
    }
}
