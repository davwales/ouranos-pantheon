using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.SetItemCategory.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.SetItemCategory;

public sealed class SetItemCategoryEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOkWithSetCategory()
    {
        // Arrange
        var ct = CancellationToken.None;
        var body = new SetItemCategoryBody("Milk", ShoppingCategory.DairyAndEggs);
        var expected = new SetItemCategoryResponse("Milk", ShoppingCategory.DairyAndEggs);
        _bus.InvokeAsync<SetItemCategoryResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await SetItemCategoryEndpoint.Handle(body, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<SetItemCategoryResponse>>().Value.ShouldBe(expected);
        await _bus.Received(1)
            .InvokeAsync<SetItemCategoryResponse>(
                Arg.Is<SetItemCategoryInput>(input =>
                    input.Name == "Milk" && input.Category == ShoppingCategory.DairyAndEggs
                ),
                ct
            );
    }
}
