using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;

public static class GetAllSymbolsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbols", Handle)
            .CacheOutput(policy =>
                policy
                    .Expire(TimeSpan.FromSeconds(30))
                    .SetVaryByQuery(
                        nameof(GetAllSymbolsInput.SortField),
                        nameof(GetAllSymbolsInput.SortDirection),
                        nameof(GetAllSymbolsInput.Skip),
                        nameof(GetAllSymbolsInput.Take),
                        nameof(GetAllSymbolsInput.Filter)
                    )
            )
            .WithTags("Plutus.Symbols");
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
