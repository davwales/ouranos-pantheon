using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory;

public static class GetRecipeHistoryEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hestia/recipes/{recipeId}/history", Handle).WithTags("Hestia.Recipes");
    }

    internal static async Task<IResult> Handle(
        Id<Recipe> recipeId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<GetRecipeHistoryResponse>(
            new GetRecipeHistoryInput(recipeId),
            ct
        );

        return Results.Ok(result);
    }
}
