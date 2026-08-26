using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common;

public sealed class PagedResponseTests
{
    [Fact]
    public void PagedResponse_WhenCreated_ShouldSetAllProperties()
    {
        // Arrange
        var items = new[] { "a", "b", "c" };

        // Act
        var response = new PagedResponse<string>(items, TotalCount: 10, Skip: 0, Take: 3);

        // Assert
        response.Items.ShouldBe(items);
        response.TotalCount.ShouldBe(10);
        response.Skip.ShouldBe(0);
        response.Take.ShouldBe(3);
    }

    [Fact]
    public void PagedResponse_WhenEmpty_ShouldHaveEmptyItems()
    {
        // Act
        var response = new PagedResponse<int>([], TotalCount: 0, Skip: 0, Take: 10);

        // Assert
        response.Items.ShouldBeEmpty();
        response.TotalCount.ShouldBe(0);
    }
}
