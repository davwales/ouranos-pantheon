using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.ClearCheckedItems;

public sealed class ClearCheckedItemsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithClearedItems()
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new ClearCheckedItemsResponse([Guid.NewGuid()], []);
        _bus.InvokeAsync<ClearCheckedItemsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await ClearCheckedItemsEndpoint.Handle(_bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<ClearCheckedItemsResponse>>().Value.ShouldBe(expected);
        await _bus.Received(1)
            .InvokeAsync<ClearCheckedItemsResponse>(Arg.Any<ClearCheckedItemsInput>(), ct);
    }
}
