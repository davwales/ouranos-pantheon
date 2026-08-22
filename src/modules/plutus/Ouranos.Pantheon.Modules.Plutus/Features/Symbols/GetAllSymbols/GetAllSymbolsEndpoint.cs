using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;

public static class GetAllSymbolsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols", Handle).WithTags("Plutus.Symbols");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllSymbolsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(input, ct));
    }
}
