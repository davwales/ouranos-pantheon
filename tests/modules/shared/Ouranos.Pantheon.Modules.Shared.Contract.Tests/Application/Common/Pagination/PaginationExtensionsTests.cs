using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Pagination;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Pagination;

public sealed class PaginationExtensionsTests
{
    private readonly IQueryable<int> _items = Enumerable.Range(1, 10).AsQueryable();

    [Fact]
    public void Paginate_WhenSkipIsZero_ShouldReturnFirstPage()
    {
        // Act
        var result = _items.Paginate(skip: 0, take: 3).ToList();

        // Assert
        result.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Paginate_WhenSkipIsPositive_ShouldOffsetResults()
    {
        // Act
        var result = _items.Paginate(skip: 3, take: 3).ToList();

        // Assert
        result.ShouldBe([4, 5, 6]);
    }

    [Fact]
    public void Paginate_WhenSkipExceedsSourceLength_ShouldReturnEmpty()
    {
        // Act
        var result = _items.Paginate(skip: 100, take: 5).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Paginate_WhenTakeExceedsRemainingItems_ShouldReturnOnlyRemaining()
    {
        // Act
        var result = _items.Paginate(skip: 8, take: 10).ToList();

        // Assert
        result.ShouldBe([9, 10]);
    }

    [Fact]
    public void Paginate_WhenLastPage_ShouldReturnCorrectItems()
    {
        // Act
        var result = _items.Paginate(skip: 9, take: 5).ToList();

        // Assert
        result.ShouldBe([10]);
    }
}
