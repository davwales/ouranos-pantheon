using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;

public static class CreateRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/plutus/recipes", Handle)
            .WithTags("Plutus.Recipes");
    }

    private static async Task<IResult> Handle(
        CreateRecipeInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Recipe>>(input, ct);
        return Results.Created($"/api/plutus/recipes/{result.Id}", result);
    }
}
