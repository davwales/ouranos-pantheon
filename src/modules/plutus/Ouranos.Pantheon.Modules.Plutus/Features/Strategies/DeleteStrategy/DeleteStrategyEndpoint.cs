using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy;

public static class DeleteStrategyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/plutus/strategies/{strategyId}", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<IdResponse<Strategy>>(new DeleteStrategyInput(strategyId), ct));
    }
}