using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;

public static class GetAllRecipesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/recipes", Handle).WithTags("Plutus.Recipes");
    }

    internal static async Task<IResult> Handle(
        [AsParameters] GetAllRecipesInput input,
        IMessageBus bus,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await bus.InvokeAsync<PagedResponse<GetAllRecipesResponse>>(input, ct));
    }
}
