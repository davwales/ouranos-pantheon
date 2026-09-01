using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems;

public sealed class UpdateCheckedItemsHandler(
    ILogger<UpdateCheckedItemsHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<UpdateCheckedItemsInput, UpdateCheckedItemsResponse>
{
    private readonly ILogger<UpdateCheckedItemsHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<UpdateCheckedItemsResponse> Handle(
        UpdateCheckedItemsInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Attempting to handle update checked items command '{@command}'.",
            command
        );
        cancellationToken.ThrowIfCancellationRequested();

        using var session = _store.LightweightSession();
        var list =
            await session.LoadAsync<ShoppingList>(ShoppingList.FixedId, cancellationToken)
            ?? new ShoppingList();

        list.CheckedItemIds = command.CheckedItemIds.Distinct().ToList();

        session.Store(list);
        await session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully updated checked items.");
        return new UpdateCheckedItemsResponse([.. list.CheckedItemIds]);
    }
}
