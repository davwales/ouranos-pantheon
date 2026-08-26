using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe;

public sealed class ReimportRecipeHandler(
    ILogger<ReimportRecipeHandler> logger,
    IHestiaMartenStore store,
    IMessageBus bus
) : IPantheonHandler<ReimportRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<ReimportRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);
    private readonly IMessageBus _bus = Guard.Against.Null(bus);

    public async Task<IdResponse<Recipe>> Handle(
        ReimportRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle reimport recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (!command.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(command.RecipeId, (Recipe?)null);
        }

        using var session = _store.LightweightSession();
        var recipe = await session.LoadAsync<Recipe>(streamId, cancellationToken);
        Guard.Against.NotFound(command.RecipeId, recipe);

        if (string.IsNullOrWhiteSpace(recipe.SourceUrl))
        {
            throw new InvalidOperationException("Recipe has no source URL to reimport from.");
        }

        var result = recipe.Reimport(DateTimeOffset.UtcNow);
        session.Events.Append(streamId, [.. result.Events]);
        await session.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(
            new ImportRecipeRequested(command.RecipeId, recipe.SourceUrl, DateTimeOffset.UtcNow)
        );

        _logger.LogDebug("Successfully reimported recipe '{recipeId}'.", command.RecipeId);
        return new IdResponse<Recipe>(command.RecipeId);
    }
}
