using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive;

public static class SetStrategyActiveEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPatch("/api/plutus/strategies/{strategyId}/active", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        Id<Strategy> strategyId,
        SetStrategyActiveBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new SetStrategyActiveInput(strategyId, body.IsActive);
        return Results.Ok(await bus.InvokeAsync<IdResponse<Strategy>>(input, ct));
    }
}

public sealed record SetStrategyActiveBody(bool IsActive);
