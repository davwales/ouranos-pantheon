using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;

public static class GetAllSymbolsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols", Handle)
            .WithTags("Plutus.Symbols");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllSymbolsInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}
