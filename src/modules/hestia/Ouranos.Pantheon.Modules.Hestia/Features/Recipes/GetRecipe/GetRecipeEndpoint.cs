using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe;

public static class GetRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hestia/recipes/{recipeId}", Handle).WithTags("Hestia.Recipes");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<GetRecipeResponse>(new GetRecipeInput(recipeId), ct)
        );
    }
}
