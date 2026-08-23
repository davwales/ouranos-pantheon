using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe;

public static class RevertRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hestia/recipes/{recipeId}/revert", Handle).WithTags("Hestia.Recipes");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        RevertRecipeBody body,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Recipe>>(
            new RevertRecipeInput(recipeId, body.TargetVersion),
            ct
        );

        return Results.Ok(result);
    }
}
