using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;

public static class GetDailySymbolSummaryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/summary", Handle)
            .CacheOutput(policy => policy.Expire(TimeSpan.FromSeconds(30)))
            .WithTags("Plutus.Symbols");
    }

    internal static async Task<IResult> Handle(
        Id<Symbol> symbolId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetDailySymbolSummaryResponse>(
            new GetDailySymbolSummaryInput(symbolId),
            ct
        );
        return Results.Ok(result);
    }
}
