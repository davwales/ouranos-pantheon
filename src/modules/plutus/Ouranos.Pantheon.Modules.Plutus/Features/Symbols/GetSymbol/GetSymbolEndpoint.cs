using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;

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
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        var result = await dispatcher.Send(new GetSymbolInput(symbolId), ct);
        return Results.Ok(result);
    }
}
