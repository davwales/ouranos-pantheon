using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem;

public sealed class AddManualItemHandler(
    ILogger<AddManualItemHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<AddManualItemInput, ManualItemResponse>
{
    private readonly ILogger<AddManualItemHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<ManualItemResponse> Handle(
        AddManualItemInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle add manual item command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.NullOrWhiteSpace(command.Text);
        Guard.Against.OutOfRange(command.Text.Length, nameof(command.Text), 1, 200);

        using var session = _store.LightweightSession();
        var list =
            await session.LoadAsync<ShoppingList>(ShoppingList.FixedId, cancellationToken)
            ?? new ShoppingList();

        var item = new ManualItem(Guid.NewGuid(), command.Text);
        list.ManualItems.Add(item);

        session.Store(list);
        await session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully added manual item '{itemId}'.", item.Id);
        return new ManualItemResponse(item.Id, item.Text);
    }
}
