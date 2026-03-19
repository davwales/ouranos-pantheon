using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Filtering;

public sealed class FilterExtensionsTests
{
    private sealed record Product(string Name, int Price, string? Category);

    private static readonly IQueryable<Product> Items = new Product[]
    {
        new Product("Sword", 100, "Weapon"),
        new Product("Shield", 50, "Armor"),
        new Product("Bow", 75, "Weapon"),
        new Product("Helmet", 30, null),
    }.AsQueryable();

    [Fact]
    public void FilterBy_WhenFiltersIsNull_ShouldReturnOriginalQuery()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Name), p => p.Name);

        // Act
        var result = Items.FilterBy(null, builder).ToList();

        // Assert
        result.Count.ShouldBe(4);
    }

    [Fact]
    public void FilterBy_WhenFilterIsEmptyString_ShouldSkipAndReturnOriginalQuery()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Name), p => p.Name);

        // Act
        var result = Items.FilterBy(["   "], builder).ToList();

        // Assert
        result.Count.ShouldBe(4);
    }

    [Fact]
    public void FilterBy_WhenEqFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Name), p => p.Name);

        // Act
        var result = Items.FilterBy(["Name:eq:Sword"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Sword");
    }

    [Fact]
    public void FilterBy_WhenLikeFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Name), p => p.Name);

        // Act
        var result = Items.FilterBy(["Name:like:ow"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Bow");
    }

    [Fact]
    public void FilterBy_WhenGtFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.FilterBy(["Price:gt:60"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(p => p.Price > 60);
    }

    [Fact]
    public void FilterBy_WhenNullFilter_ShouldReturnItemsWithNullProperty()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Category), p => p.Category);

        // Act
        var result = Items.FilterBy(["Category:null"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Helmet");
    }

    [Fact]
    public void FilterBy_WhenNotNullFilter_ShouldReturnItemsWithNonNullProperty()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Category), p => p.Category);

        // Act
        var result = Items.FilterBy(["Category:notnull"], builder).ToList();

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldAllBe(p => p.Category != null);
    }

    [Fact]
    public void FilterBy_WhenMultipleFilters_ShouldAndThemTogether()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Category), p => p.Category)
            .On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.FilterBy(["Category:eq:Weapon", "Price:gt:80"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Sword");
    }

    [Fact]
    public void FilterBy_WhenOrGroup_ShouldReturnEitherMatch()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Name), p => p.Name);

        // Act
        var result = Items.FilterBy(["or(Name:eq:Sword|Name:eq:Bow)"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.Select(p => p.Name).ShouldBe(["Sword", "Bow"], ignoreOrder: true);
    }

    [Fact]
    public void FilterBy_WhenInFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Product>()
            .On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.FilterBy(["Price:in:50,100"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.Select(p => p.Name).ShouldBe(["Sword", "Shield"], ignoreOrder: true);
    }

    [Fact]
    public void FilterBy_WhenUsingConfigureActionOverload_ShouldApplyFilter()
    {
        // Arrange & Act
        var result = Items.FilterBy(
            ["Name:eq:Helmet"],
            b => b.On(nameof(Product.Name), p => p.Name)
        ).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Helmet");
    }
}
