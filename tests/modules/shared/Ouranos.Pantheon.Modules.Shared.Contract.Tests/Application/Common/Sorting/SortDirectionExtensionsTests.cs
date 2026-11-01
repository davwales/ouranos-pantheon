using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;
using SortDirection = Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting.SortDirection;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Sorting;

public sealed class SortDirectionExtensionsTests
{
    [Theory]
    [InlineData(SortDirection.Asc, "asc")]
    [InlineData(SortDirection.Desc, "desc")]
    public void ToQueryValue_ShouldReturnLowerCaseDirection(
        SortDirection sortDirection,
        string expected
    )
    {
        // Act
        var value = sortDirection.ToQueryValue();

        // Assert
        value.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SortDirection.Asc)]
    [InlineData(SortDirection.Desc)]
    public void ToQueryValue_ShouldBeAcceptedBySortBy(SortDirection sortDirection)
    {
        // Arrange
        var items = new[] { 2, 1, 3 }.AsQueryable();
        var builder = new SortBuilder<int>().On("Value", x => x);

        // Act
        var result = items.SortBy("Value", sortDirection.ToQueryValue(), builder).ToList();

        // Assert
        result.ShouldBe(sortDirection == SortDirection.Asc ? [1, 2, 3] : [3, 2, 1]);
    }
}
