using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.UpdateCheckedItems;

public sealed class UpdateCheckedItemsHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly UpdateCheckedItemsHandler _handler;

    public UpdateCheckedItemsHandlerTests()
    {
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);
        _handler = new UpdateCheckedItemsHandler(
            Substitute.For<ILogger<UpdateCheckedItemsHandler>>(),
            _store
        );
    }

    [Fact]
    public async Task Handle_WhenCheckedIdsProvided_ShouldStoreVerbatimAndEcho()
    {
        // Arrange
        var ids = new List<string>
        {
            "recipe:flour|g",
            "manual:11111111-1111-1111-1111-111111111111",
        };
        var list = new ShoppingList();
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var result = await _handler.Handle(
            new UpdateCheckedItemsInput(ids),
            CancellationToken.None
        );

        // Assert
        result.CheckedItemIds.Count.ShouldBe(2);
        result.CheckedItemIds[0].ShouldBe("recipe:flour|g");
        result.CheckedItemIds[1].ShouldBe("manual:11111111-1111-1111-1111-111111111111");
        list.CheckedItemIds.Count.ShouldBe(2);
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDuplicateIdsProvided_ShouldStoreDistinctOnly()
    {
        // Arrange
        var ids = new List<string>
        {
            "recipe:flour|g",
            "recipe:flour|g",
            "manual:22222222-2222-2222-2222-222222222222",
        };
        var list = new ShoppingList();
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var result = await _handler.Handle(
            new UpdateCheckedItemsInput(ids),
            CancellationToken.None
        );

        // Assert
        result.CheckedItemIds.Count.ShouldBe(2);
        result.CheckedItemIds.Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WhenListDoesNotExist_ShouldCreateAndStoreIds()
    {
        // Arrange
        var ids = new List<string> { "recipe:flour|g" };
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(null));

        // Act
        var result = await _handler.Handle(
            new UpdateCheckedItemsInput(ids),
            CancellationToken.None
        );

        // Assert
        result.CheckedItemIds.Count.ShouldBe(1);
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var ct = new CancellationToken(true);

        // Act
        var update = async () =>
            await _handler.Handle(new UpdateCheckedItemsInput(new List<string>()), ct);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
