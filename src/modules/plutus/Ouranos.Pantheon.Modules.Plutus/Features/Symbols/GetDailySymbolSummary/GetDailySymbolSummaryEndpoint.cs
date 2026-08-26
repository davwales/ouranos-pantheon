using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;

public static class GetDailySymbolSummaryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}/summary", Handle).WithTags("Plutus.Symbols");
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
