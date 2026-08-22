using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup;

public static class DeleteSymbolGroupEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/plutus/symbol-groups/{symbolGroupId}", Handle)
            .WithTags("Plutus.SymbolGroups");
    }

    internal static async Task<IResult> Handle(
        Id<SymbolGroup> symbolGroupId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<IdResponse<SymbolGroup>>(
                new DeleteSymbolGroupInput(symbolGroupId),
                ct
            )
        );
    }
}
