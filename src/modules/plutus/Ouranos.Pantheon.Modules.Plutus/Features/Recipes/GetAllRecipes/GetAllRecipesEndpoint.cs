using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;

public static class GetAllRecipesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/plutus/recipes", Handle)
            .WithTags("Plutus.Recipes");
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllRecipesInput input,
        IScopedDispatcher dispatcher,
        CancellationToken ct = default
    )
    {
        return Results.Ok(await dispatcher.Send(input, ct));
    }
}
