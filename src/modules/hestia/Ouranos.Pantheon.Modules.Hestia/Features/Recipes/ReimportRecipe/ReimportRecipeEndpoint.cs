using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe;

public static class ReimportRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hestia/recipes/{recipeId}/reimport", Handle).WithTags("Hestia.Recipes");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var input = new ReimportRecipeInput(recipeId);
        var result = await bus.InvokeAsync<IdResponse<Recipe>>(input, ct);
        return Results.Accepted($"/api/hestia/recipes/{result.Id}", result);
    }
}
