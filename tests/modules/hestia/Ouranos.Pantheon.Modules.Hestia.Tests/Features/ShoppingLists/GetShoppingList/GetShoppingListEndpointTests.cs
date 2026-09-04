using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.GetShoppingList;

public sealed class GetShoppingListEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithShoppingListResponse()
    {
        // Arrange
        var ct = CancellationToken.None;
        var expected = new ShoppingListResponse([], [], [], [], []);

        _bus.InvokeAsync<ShoppingListResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetShoppingListEndpoint.Handle(_bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<ShoppingListResponse>>();
        await _bus.Received(1)
            .InvokeAsync<ShoppingListResponse>(Arg.Is<GetShoppingListInput>(_ => true), ct);
    }
}
