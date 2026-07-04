using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe;

public static class CreateRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hestia/recipes", Handle).WithTags("Hestia.Recipes");
    }

    internal static async Task<IResult> Handle(
        CreateRecipeInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Recipe>>(input, ct);
        return Results.Created($"/api/hestia/recipes/{result.Id}", result);
    }
}
