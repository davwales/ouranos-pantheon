using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup;

public static class CreateSymbolGroupEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/symbol-groups", Handle).WithTags("Plutus.SymbolGroups");
    }

    internal static async Task<IResult> Handle(
        CreateSymbolGroupInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<SymbolGroup>>(input, ct);
        return Results.Created($"/api/plutus/symbol-groups/{result.Id}", result);
    }
}
