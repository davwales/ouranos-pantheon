using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals;

public static class GetSymbolSignalsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/signals", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(nameof(GetSymbolSignalsInput.Intent))
            )
            .WithTags("Plutus.Signals");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetSymbolSignalsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<GetSymbolSignalsResponse>(input, ct));
    }
}
