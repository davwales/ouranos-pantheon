using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;

public static class UpdateRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/plutus/recipes/{recipeId}", Handle)
            .WithTags("Plutus.Recipes");
    }

    private static async Task<IResult> Handle(
        UpdateRecipeInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}
