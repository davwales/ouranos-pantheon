using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;

public static class GetRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/recipes/{recipeId}", Handle)
            .WithTags("Plutus.Recipes");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetRecipeResponse>(new GetRecipeInput(recipeId), ct);
        return Results.Ok(result);
    }
}
