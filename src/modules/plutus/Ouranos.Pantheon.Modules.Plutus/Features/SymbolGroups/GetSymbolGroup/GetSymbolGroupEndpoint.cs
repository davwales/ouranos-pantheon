using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup;

public static class GetSymbolGroupEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/symbol-groups/{symbolGroupId}", Handle)
            .WithTags("Plutus.SymbolGroups");
    }

    internal static async Task<IResult> Handle(
        Id<SymbolGroup> symbolGroupId,
        TimeFrame timeFrame,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetSymbolGroupResponse>(
            new GetSymbolGroupInput(symbolGroupId, timeFrame),
            ct
        );
        return Results.Ok(result);
    }
}
