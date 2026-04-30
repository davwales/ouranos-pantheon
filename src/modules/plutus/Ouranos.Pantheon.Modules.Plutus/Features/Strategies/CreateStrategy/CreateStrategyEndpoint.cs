using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy;

public static class CreateStrategyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/strategies", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        CreateStrategyInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Strategy>>(input, ct);
        return Results.Created($"/api/plutus/strategies/{result.Id}", result);
    }
}