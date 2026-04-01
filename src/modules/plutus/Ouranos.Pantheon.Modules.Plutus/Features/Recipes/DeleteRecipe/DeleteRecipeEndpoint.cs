using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;

public static class DeleteRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/plutus/recipes/{recipeId}", Handle)
            .WithTags("Plutus.Recipes");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<IdResponse<Recipe>>(new DeleteRecipeInput(recipeId), ct));
    }
}
