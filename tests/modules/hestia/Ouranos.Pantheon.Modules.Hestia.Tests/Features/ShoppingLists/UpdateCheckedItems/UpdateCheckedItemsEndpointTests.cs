using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems.Schemas;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.UpdateCheckedItems;

public sealed class UpdateCheckedItemsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithUpdateResponse()
    {
        // Arrange
        var ct = CancellationToken.None;
        var ids = new List<string> { "recipe:flour|g" };
        var body = new UpdateCheckedItemsBody(ids);
        var expected = new UpdateCheckedItemsResponse(ids);

        _bus.InvokeAsync<UpdateCheckedItemsResponse>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await UpdateCheckedItemsEndpoint.Handle(body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<UpdateCheckedItemsResponse>>();
        await _bus.Received(1)
            .InvokeAsync<UpdateCheckedItemsResponse>(
                Arg.Is<UpdateCheckedItemsInput>(input => input.CheckedItemIds == ids),
                ct
            );
    }
}
