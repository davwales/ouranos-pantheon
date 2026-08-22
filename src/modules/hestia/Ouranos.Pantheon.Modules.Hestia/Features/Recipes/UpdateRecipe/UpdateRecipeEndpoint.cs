using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe;

public static class UpdateRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/hestia/recipes/{recipeId}", Handle).WithTags("Hestia.Recipes");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        UpdateRecipeBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new UpdateRecipeInput(
            recipeId,
            body.Title,
            body.SourceUrl,
            body.Steps,
            body.Ingredients,
            body.Notes
        );

        return Results.Ok(await bus.InvokeAsync<IdResponse<Recipe>>(input, ct));
    }
}
