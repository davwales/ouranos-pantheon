using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeHandler(
    ILogger<CreateRecipeHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<CreateRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<CreateRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<IdResponse<Recipe>> Handle(
        CreateRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle create recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var result = Recipe.Create(
            Guid.NewGuid(),
            command.Title,
            command.SourceUrl,
            [.. command.Steps.Select(s => new Step(s.Text))],
            [.. command.Ingredients.Select(i => new Ingredient(i.Quantity, i.Unit, i.Name))],
            command.Notes ?? string.Empty
        );

        using var session = _store.LightweightSession();
        session.Events.StartStream(result.State.Id, [.. result.Events]);
        await session.SaveChangesAsync(cancellationToken);

        var response = new IdResponse<Recipe>(result.State.RecipeId);

        _logger.LogDebug(
            "Successfully handled create recipe request for recipe '{recipeId}'.",
            result.State.RecipeId
        );
        return response;
    }
}
