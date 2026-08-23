using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;

public static class ImportRecipeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/hestia/recipes/import", Handle).WithTags("Hestia.Recipes");
    }

    internal static async Task<IResult> Handle(
        ImportRecipeInput input,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var result = await bus.InvokeAsync<IdResponse<Recipe>>(input, ct);
        return Results.Accepted($"/api/hestia/recipes/{result.Id}", result);
    }
}
