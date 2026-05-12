using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy;

public static class UpdateStrategyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/plutus/strategies/{strategyId}", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        UpdateStrategyBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdateStrategyInput(
            strategyId,
            body.Name,
            body.Description,
            body.Configuration,
            body.SignalWeightedConfig,
            body.ForecastMomentumConfig,
            body.MeanReversionConfig,
            body.RecipeArbitrageConfig
        );
        return Results.Ok(await bus.InvokeAsync<IdResponse<Strategy>>(input, ct));
    }
}