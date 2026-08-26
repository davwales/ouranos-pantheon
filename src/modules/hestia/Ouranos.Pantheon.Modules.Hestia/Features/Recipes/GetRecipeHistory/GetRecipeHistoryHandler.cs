using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Extensions;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory;

public sealed class GetRecipeHistoryHandler(
    ILogger<GetRecipeHistoryHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<GetRecipeHistoryInput, GetRecipeHistoryResponse>
{
    private readonly ILogger<GetRecipeHistoryHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<GetRecipeHistoryResponse> Handle(
        GetRecipeHistoryInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get recipe history query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        if (!query.RecipeId.TryGetStreamId(out var streamId))
        {
            Guard.Against.NotFound(query.RecipeId, (Recipe?)null);
        }

        using var session = _store.QuerySession();
        var streamState = await session.Events.FetchStreamStateAsync(streamId, cancellationToken);
        Guard.Against.NotFound(query.RecipeId, streamState);

        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken);

        var historyEvents = events
            .Select(e => new RecipeHistoryEventResponse(e.Version, e.EventTypeName, e.Timestamp))
            .ToList();

        _logger.LogDebug(
            "Successfully handled get recipe history request for recipe '{recipeId}'.",
            query.RecipeId
        );
        return new GetRecipeHistoryResponse(query.RecipeId, historyEvents);
    }
}
