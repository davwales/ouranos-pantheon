using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.AddManualItem;

public sealed class AddManualItemHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly AddManualItemHandler _handler;

    public AddManualItemHandlerTests()
    {
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);
        _handler = new AddManualItemHandler(
            Substitute.For<ILogger<AddManualItemHandler>>(),
            _store
        );
    }

    [Fact]
    public async Task Handle_WhenListExists_ShouldAppendItemAndReturnResponse()
    {
        // Arrange
        var existingItem = new ManualItem(Guid.NewGuid(), "Salt");
        var list = new ShoppingList { ManualItems = [existingItem] };
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var result = await _handler.Handle(new AddManualItemInput("Milk"), CancellationToken.None);

        // Assert
        result.Text.ShouldBe("Milk");
        list.ManualItems.Count.ShouldBe(2);
        list.ManualItems.ShouldContain(i => i.Text == "Milk");
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenListDoesNotExist_ShouldCreateAndAppendItem()
    {
        // Arrange
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(null));

        // Act
        var result = await _handler.Handle(new AddManualItemInput("Eggs"), CancellationToken.None);

        // Assert
        result.Text.ShouldBe("Eggs");
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTextIsWhiteSpace_ShouldThrowArgumentException()
    {
        // Arrange
        var input = new AddManualItemInput("   ");

        // Act
        var add = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await add.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenTextIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var input = new AddManualItemInput("");

        // Act
        var add = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await add.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenTextExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var input = new AddManualItemInput(new string('a', 201));

        // Act
        var add = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await add.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var ct = new CancellationToken(true);

        // Act
        var add = async () => await _handler.Handle(new AddManualItemInput("Milk"), ct);

        // Assert
        await add.ShouldThrowAsync<OperationCanceledException>();
    }
}
