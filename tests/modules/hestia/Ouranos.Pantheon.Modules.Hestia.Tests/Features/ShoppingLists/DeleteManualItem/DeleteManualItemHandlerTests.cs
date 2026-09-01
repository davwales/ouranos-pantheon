using Ardalis.GuardClauses;
using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.DeleteManualItem;

public sealed class DeleteManualItemHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly DeleteManualItemHandler _handler;

    public DeleteManualItemHandlerTests()
    {
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);
        _handler = new DeleteManualItemHandler(
            Substitute.For<ILogger<DeleteManualItemHandler>>(),
            _store
        );
    }

    [Fact]
    public async Task Handle_WhenItemExists_ShouldRemoveAndReturnId()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new ManualItem(itemId, "Milk");
        var list = new ShoppingList { ManualItems = [item] };

        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var result = await _handler.Handle(
            new DeleteManualItemInput(itemId),
            CancellationToken.None
        );

        // Assert
        result.Id.ShouldBe(itemId);
        list.ManualItems.ShouldBeEmpty();
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenItemChecked_ShouldRemoveCheckedKey()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new ManualItem(itemId, "Milk");
        var checkedKey = ShoppingListNormalizer.ManualLineKey(itemId);
        var list = new ShoppingList { ManualItems = [item], CheckedItemIds = [checkedKey] };

        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        await _handler.Handle(new DeleteManualItemInput(itemId), CancellationToken.None);

        // Assert
        list.CheckedItemIds.ShouldNotContain(checkedKey);
    }

    [Fact]
    public async Task Handle_WhenItemMissing_ShouldThrowNotFoundException()
    {
        // Arrange
        var list = new ShoppingList();
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var delete = async () =>
            await _handler.Handle(
                new DeleteManualItemInput(Guid.NewGuid()),
                CancellationToken.None
            );

        // Assert
        await delete.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenListDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(null));

        // Act
        var delete = async () =>
            await _handler.Handle(
                new DeleteManualItemInput(Guid.NewGuid()),
                CancellationToken.None
            );

        // Assert
        await delete.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var ct = new CancellationToken(true);

        // Act
        var delete = async () =>
            await _handler.Handle(new DeleteManualItemInput(Guid.NewGuid()), ct);

        // Assert
        await delete.ShouldThrowAsync<OperationCanceledException>();
    }
}
