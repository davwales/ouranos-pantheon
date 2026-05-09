using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Filtering;

/// <summary>
/// Tests for additional filter operators (startswith, endswith, ne, lte, gte)
/// that are not covered by FilterExtensionsTests.
/// </summary>
public sealed class FilterOperatorExtensionsTests
{
    private sealed record Product(string Name, int Price, string? Category);

    private static readonly IQueryable<Product> Items = new Product[]
    {
        new("Sword", 100, "Weapon"),
        new("Shield", 50, "Armor"),
        new("Bow", 75, "Weapon"),
        new("Helmet", 30, null),
    }.AsQueryable();

    [Fact]
    public void FilterBy_WhenStartsWithFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Name), p => p.Name);

        // Act
        var result = Items.FilterBy(["Name:startswith:Sw"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Sword");
    }

    [Fact]
    public void FilterBy_WhenEndsWithFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Name), p => p.Name);

        // Act
        var result = Items.FilterBy(["Name:endswith:eld"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Shield");
    }

    [Fact]
    public void FilterBy_WhenNeFilter_ShouldReturnNonMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Category), p => p.Category);

        // Act
        var result = Items.FilterBy(["Category:neq:Weapon"], builder).ToList();

        // Assert
        result.ShouldAllBe(p => p.Category != "Weapon");
    }

    [Fact]
    public void FilterBy_WhenLteFilter_ShouldReturnItemsLessThanOrEqual()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.FilterBy(["Price:lte:50"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(p => p.Price <= 50);
    }

    [Fact]
    public void FilterBy_WhenGteFilter_ShouldReturnItemsGreaterThanOrEqual()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.FilterBy(["Price:gte:75"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(p => p.Price >= 75);
    }

    [Fact]
    public void FilterBy_WhenLtFilter_ShouldReturnItemsLessThan()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.FilterBy(["Price:lt:75"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(p => p.Price < 75);
    }

}
