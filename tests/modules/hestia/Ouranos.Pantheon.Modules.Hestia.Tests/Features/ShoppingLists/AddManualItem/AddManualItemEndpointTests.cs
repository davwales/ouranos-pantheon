using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.AddManualItem;

public sealed class AddManualItemEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithManualItemResponse()
    {
        // Arrange
        var ct = CancellationToken.None;
        var body = new AddManualItemBody("Milk");
        var expected = new ManualItemResponse(Guid.NewGuid(), "Milk");

        _bus.InvokeAsync<ManualItemResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await AddManualItemEndpoint.Handle(body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<ManualItemResponse>>();
        await _bus.Received(1)
            .InvokeAsync<ManualItemResponse>(
                Arg.Is<AddManualItemInput>(input => input.Text == "Milk"),
                ct
            );
    }
}
