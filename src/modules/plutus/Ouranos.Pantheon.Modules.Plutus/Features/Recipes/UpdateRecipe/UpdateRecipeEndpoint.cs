using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;

public static class UpdateRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/api/plutus/recipes/{recipeId}", Handle)
            .WithTags("Plutus.Recipes");
    }

    internal static async Task<IResult> Handle(
        UpdateRecipeInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        return Results.Ok(await bus.InvokeAsync<IdResponse<Recipe>>(input, ct));
    }
}
