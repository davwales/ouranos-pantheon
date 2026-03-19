using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using SortDirection = Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting.SortDirection;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Sorting;

public sealed class SortBuilderTests
{
    private sealed record Entry(string Name, int Score);

    private static readonly IQueryable<Entry> Items = new Entry[]
    {
        new("Banana", 30), new("Apple", 10), new("Cherry", 20),
    }.AsQueryable();

    [Fact]
    public void SortBy_WhenFieldMatchesAscending_ShouldReturnAscendingOrder()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .On(nameof(Entry.Name), e => e.Name);

        // Act
        var result = Items.SortBy("Name", "asc", builder).ToList();

        // Assert
        result.Select(e => e.Name).ShouldBe(["Apple", "Banana", "Cherry"]);
    }

    [Fact]
    public void SortBy_WhenFieldMatchesDescending_ShouldReturnDescendingOrder()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .On(nameof(Entry.Name), e => e.Name);

        // Act
        var result = Items.SortBy("Name", "desc", builder).ToList();

        // Assert
        result.Select(e => e.Name).ShouldBe(["Cherry", "Banana", "Apple"]);
    }

    [Fact]
    public void SortBy_WhenFieldMatchesAndDirectionIsAscCaseInsensitive_ShouldSortAscending()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .On(nameof(Entry.Score), e => e.Score);

        // Act
        var result = Items.SortBy("Score", "ASC", builder).ToList();

        // Assert
        result.Select(e => e.Score).ShouldBe([10, 20, 30]);
    }

    [Fact]
    public void SortBy_WhenFieldIsUnknown_ShouldFallBackToDefault()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .On(nameof(Entry.Name), e => e.Name)
            .Default(e => e.Score, SortDirection.Asc);

        // Act
        var result = Items.SortBy("Unknown", "asc", builder).ToList();

        // Assert
        result.Select(e => e.Score).ShouldBe([10, 20, 30]);
    }

    [Fact]
    public void SortBy_WhenFieldIsNull_ShouldFallBackToDefault()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .Default(e => e.Score);

        // Act
        var result = Items.SortBy(null, null, builder).ToList();

        // Assert
        result.Select(e => e.Score).ShouldBe([30, 20, 10]);
    }

    [Fact]
    public void SortBy_WhenNoDefaultAndFieldIsUnknown_ShouldReturnUnsortedQuery()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .On(nameof(Entry.Name), e => e.Name);

        var expected = Items.ToList();

        // Act
        var result = Items.SortBy("Unknown", "asc", builder).ToList();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void SortBy_WhenFieldLookupIsCaseInsensitive_ShouldSortCorrectly()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .On(nameof(Entry.Name), e => e.Name);

        // Act
        var result = Items.SortBy("name", "asc", builder).ToList();

        // Assert
        result.Select(e => e.Name).ShouldBe(["Apple", "Banana", "Cherry"]);
    }

    [Fact]
    public void SortBy_WhenUsingConfigureActionOverload_ShouldSortCorrectly()
    {
        // Arrange & Act
        var result = Items.SortBy(
            nameof(Entry.Score),
            "asc",
            b => b.On(nameof(Entry.Score), e => e.Score)
        ).ToList();

        // Assert
        result.Select(e => e.Score).ShouldBe([10, 20, 30]);
    }

    [Fact]
    public void Default_WhenDesc_ShouldSortDescending()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .Default(e => e.Score, sortDirection: SortDirection.Desc);

        // Act
        var result = Items.SortBy(null, null, builder).ToList();

        // Assert
        result.Select(e => e.Score).ShouldBe([30, 20, 10]);
    }

    [Fact]
    public void Default_WhenAsc_ShouldSortAscending()
    {
        // Arrange
        var builder = new SortBuilder<Entry>()
            .Default(e => e.Score, SortDirection.Asc);

        // Act
        var result = Items.SortBy(null, null, builder).ToList();

        // Assert
        result.Select(e => e.Score).ShouldBe([10, 20, 30]);
    }
}
