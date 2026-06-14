using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory;

public static class GetSymbolSignalHistoryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/signal-history", Handle)
            .WithTags("Plutus.Signals");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetSymbolSignalHistoryInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<GetSymbolSignalHistoryResponse>(input, ct));
    }
}
