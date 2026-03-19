using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;

public static class DeleteRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/plutus/recipes/{recipeId}", Handle)
            .WithTags("Plutus.Recipes");
    }

    private static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        return Results.Ok(await dispatcher.Send(new DeleteRecipeInput(recipeId), ct));
    }
}
