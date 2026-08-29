using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe;

public static class ToggleRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hestia/shopping-list/recipes/{recipeId}", Handle)
            .WithTags("Hestia.ShoppingList");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(
            await bus.InvokeAsync<ToggleRecipeResponse>(new ToggleRecipeInput(recipeId), ct)
        );
    }
}
