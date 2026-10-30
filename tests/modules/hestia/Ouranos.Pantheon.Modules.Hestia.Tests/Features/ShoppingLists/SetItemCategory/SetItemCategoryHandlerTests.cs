using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.SetItemCategory;

public sealed class SetItemCategoryHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly SetItemCategoryHandler _handler;

    public SetItemCategoryHandlerTests()
    {
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);
        _handler = new SetItemCategoryHandler(
            Substitute.For<ILogger<SetItemCategoryHandler>>(),
            _store
        );
    }

    private ShoppingList GivenList(ShoppingList list)
    {
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));
        return list;
    }

    [Fact]
    public async Task Handle_WhenCategoryGiven_ShouldStoreItByNameKey()
    {
        // Arrange
        var list = GivenList(new ShoppingList());

        // Act
        var result = await _handler.Handle(
            new SetItemCategoryInput("Large Eggs", ShoppingCategory.DairyAndEggs),
            CancellationToken.None
        );

        // Assert
        list.Categories["large egg"].ShouldBe(ShoppingCategory.DairyAndEggs);
        result.Category.ShouldBe(ShoppingCategory.DairyAndEggs);
        _session.Received(1).Store(list);
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCategoryNull_ShouldForgetTheCategory()
    {
        // Arrange
        var list = GivenList(
            new ShoppingList { Categories = new() { ["milk"] = ShoppingCategory.DairyAndEggs } }
        );

        // Act
        await _handler.Handle(new SetItemCategoryInput("Milk", null), CancellationToken.None);

        // Assert
        list.Categories.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNameBlank_ShouldThrowArgumentException()
    {
        // Arrange
        GivenList(new ShoppingList());

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() =>
            _handler.Handle(
                new SetItemCategoryInput("  ", ShoppingCategory.Produce),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SetItemCategoryHandler(Substitute.For<ILogger<SetItemCategoryHandler>>(), null!)
        );
    }
}
