using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory;

public static class GetSymbolSignalHistoryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/signal-history", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(
                        nameof(GetSymbolSignalHistoryInput.From),
                        nameof(GetSymbolSignalHistoryInput.To),
                        nameof(GetSymbolSignalHistoryInput.Types),
                        nameof(GetSymbolSignalHistoryInput.Intent)
                    )
            )
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
