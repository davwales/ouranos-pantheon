using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup;

public static class UpdateSymbolGroupEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/plutus/symbol-groups/{symbolGroupId}", Handle)
            .WithTags("Plutus.SymbolGroups");
    }

    internal static async Task<IResult> Handle(
        UpdateSymbolGroupInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<IdResponse<SymbolGroup>>(input, ct));
    }
}
