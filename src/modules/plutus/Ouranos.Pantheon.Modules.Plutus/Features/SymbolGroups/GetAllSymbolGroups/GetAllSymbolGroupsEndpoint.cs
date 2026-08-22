using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups;

public static class GetAllSymbolGroupsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbol-groups", Handle).WithTags("Plutus.SymbolGroups");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllSymbolGroupsInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<PagedResponse<GetAllSymbolGroupsResponse>>(input, ct)
        );
    }
}
