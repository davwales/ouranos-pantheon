using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies;

public static class GetAllStrategiesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/strategies", Handle)
            .WithTags("Plutus.Strategies");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllStrategiesInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetAllStrategiesResponse>>(input, ct));
    }
}