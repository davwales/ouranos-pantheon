using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

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
        IScopedDispatcher dispatcher,
        CancellationToken ct
    )
    {
        var result = await dispatcher.Send(input, ct);
        return Results.Created($"/api/plutus/recipes/{result.Id}", result);
    }
}
