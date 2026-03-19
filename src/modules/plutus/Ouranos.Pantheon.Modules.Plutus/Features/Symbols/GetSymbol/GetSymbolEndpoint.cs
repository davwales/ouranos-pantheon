using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol;

public static class GetSymbolEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols/{symbolId}", Handle)
            .WithTags("Plutus.Symbols");
    }

    private static async Task<IResult> Handle(
        Id<Symbol> symbolId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetSymbolResponse>(new GetSymbolInput(symbolId), ct);
        return Results.Ok(result);
    }
}
