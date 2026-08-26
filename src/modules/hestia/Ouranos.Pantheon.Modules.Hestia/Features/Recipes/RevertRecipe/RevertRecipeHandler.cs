using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe;

public sealed class RevertRecipeHandler(
    ILogger<RevertRecipeHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<RevertRecipeInput, IdResponse<Recipe>>
{
    private readonly ILogger<RevertRecipeHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<IdResponse<Recipe>> Handle(
        RevertRecipeInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle revert recipe command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        if (!command.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(command.RecipeId, (Recipe?)null);
        }

        using var session = _store.LightweightSession();

        var streamState = await session.Events.FetchStreamStateAsync(streamId, cancellationToken);
        Guard.Against.NotFound(command.RecipeId, streamState);

        Guard.Against.OutOfRange(
            command.TargetVersion,
            nameof(command.TargetVersion),
            1,
            long.MaxValue
        );

        if (command.TargetVersion > streamState.Version)
        {
            Guard.Against.NotFound(command.TargetVersion, (Recipe?)null);
        }

        if (command.TargetVersion == streamState.Version)
        {
            return new IdResponse<Recipe>(command.RecipeId);
        }

        var current = await session.LoadAsync<Recipe>(streamId, cancellationToken);
        Guard.Against.NotFound(command.RecipeId, current);

        var historical = await session.Events.AggregateStreamAsync<Recipe>(
            streamId,
            version: command.TargetVersion,
            token: cancellationToken
        );
        Guard.Against.Null(historical, nameof(historical));

        var result = current.Revert(command.TargetVersion, historical, DateTimeOffset.UtcNow);
        session.Events.Append(streamId, [.. result.Events]);
        await session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully reverted recipe '{recipeId}' to target version '{targetVersion}'.",
            command.RecipeId,
            command.TargetVersion
        );
        return new IdResponse<Recipe>(command.RecipeId);
    }
}
