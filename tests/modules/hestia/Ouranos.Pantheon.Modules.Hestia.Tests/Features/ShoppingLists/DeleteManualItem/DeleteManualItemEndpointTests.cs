using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.DeleteManualItem;

public sealed class DeleteManualItemEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithDeleteResponse()
    {
        // Arrange
        var ct = CancellationToken.None;
        var itemId = Guid.NewGuid();
        var expected = new DeleteManualItemResponse(itemId);

        _bus.InvokeAsync<DeleteManualItemResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteManualItemEndpoint.Handle(itemId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<DeleteManualItemResponse>>();
        await _bus.Received(1)
            .InvokeAsync<DeleteManualItemResponse>(
                Arg.Is<DeleteManualItemInput>(input => input.ItemId == itemId),
                ct
            );
    }
}
