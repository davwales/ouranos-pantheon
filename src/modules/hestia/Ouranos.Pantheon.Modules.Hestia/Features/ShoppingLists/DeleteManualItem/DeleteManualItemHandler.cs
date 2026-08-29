using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem;

public sealed class DeleteManualItemHandler(
    ILogger<DeleteManualItemHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<DeleteManualItemInput, DeleteManualItemResponse>
{
    private readonly ILogger<DeleteManualItemHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<DeleteManualItemResponse> Handle(
        DeleteManualItemInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle delete manual item command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        using var session = _store.LightweightSession();
        var list =
            await session.LoadAsync<ShoppingList>(ShoppingList.FixedId, cancellationToken)
            ?? new ShoppingList();

        var item = list.ManualItems.FirstOrDefault(i => i.Id == command.ItemId);
        Guard.Against.NotFound(command.ItemId, item);

        list.ManualItems.Remove(item);
        list.CheckedItemIds.Remove(ShoppingListNormalizer.ManualLineKey(item.Id));

        session.Store(list);
        await session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully deleted manual item '{itemId}'.", command.ItemId);
        return new DeleteManualItemResponse(command.ItemId);
    }
}
