using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory;

public sealed class SetItemCategoryHandler(
    ILogger<SetItemCategoryHandler> logger,
    IHestiaMartenStore store
) : IPantheonHandler<SetItemCategoryInput, SetItemCategoryResponse>
{
    private readonly ILogger<SetItemCategoryHandler> _logger = Guard.Against.Null(logger);
    private readonly IHestiaMartenStore _store = Guard.Against.Null(store);

    public async Task<SetItemCategoryResponse> Handle(
        SetItemCategoryInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle set item category command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        Guard.Against.NullOrWhiteSpace(command.Name);
        Guard.Against.OutOfRange(command.Name.Length, nameof(command.Name), 1, 200);

        using var session = _store.LightweightSession();
        var list =
            await session.LoadAsync<ShoppingList>(ShoppingList.FixedId, cancellationToken)
            ?? new ShoppingList();

        var nameKey = ShoppingListNormalizer.NameKey(command.Name);
        if (command.Category is { } category)
        {
            list.Categories[nameKey] = category;
        }
        else
        {
            list.Categories.Remove(nameKey);
        }

        session.Store(list);
        await session.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Successfully set category of '{nameKey}' to '{category}'.",
            nameKey,
            command.Category
        );
        return new SetItemCategoryResponse(command.Name, command.Category);
    }
}
