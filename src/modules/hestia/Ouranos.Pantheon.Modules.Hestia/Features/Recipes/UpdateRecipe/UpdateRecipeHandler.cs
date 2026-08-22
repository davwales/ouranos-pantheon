using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandler(
    ILogger<UpdateRecipeHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<UpdateRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<UpdateRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<IdResponse<Recipe>> Handle(
        UpdateRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle update recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (!command.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(command.RecipeId, (Recipe?)null);
        }

        using var session = _store.LightweightSession();
        var current = await session.LoadAsync<Recipe>(streamId, cancellationToken);
        Guard.Against.NotFound(command.RecipeId, current);

        var result = current.Update(
            command.Title,
            command.SourceUrl,
            [.. command.Steps.Select(s => new Step(s.Text))],
            [.. command.Ingredients.Select(i => new Ingredient(i.Quantity, i.Unit, i.Name))],
            command.Notes ?? string.Empty
        );

        if (result.Events.Count > 0)
        {
            session.Events.Append(streamId, [.. result.Events]);
            await session.SaveChangesAsync(cancellationToken);
        }

        var response = new IdResponse<Recipe>(result.State.RecipeId);

        _logger.LogDebug(
            "Successfully handled update recipe request for recipe '{recipeId}'.",
            result.State.RecipeId
        );
        return response;
    }
}
