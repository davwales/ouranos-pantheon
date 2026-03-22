using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals;

public static class GetSymbolSignalsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/signals", Handle)
            .WithTags("Plutus.Signals");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetSymbolSignalsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<GetSymbolSignalsResponse>(input, ct));
    }
}
