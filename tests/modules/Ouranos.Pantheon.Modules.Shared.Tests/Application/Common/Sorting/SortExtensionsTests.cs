using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Sorting;

public sealed class SortExtensionsTests
{
    private sealed record Product(string Name, int Price);

    private static readonly IQueryable<Product> Items = new Product[]
    {
        new("Sword", 100),
        new("Shield", 50),
        new("Bow", 75),
    }.AsQueryable();

    [Fact]
    public void SortBy_WithBuilder_ShouldSortAscending()
    {
        // Arrange
        var builder = new SortBuilder<Product>().On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.SortBy("Price", "asc", builder).ToList();

        // Assert
        result[0].Price.ShouldBe(50);
        result[1].Price.ShouldBe(75);
        result[2].Price.ShouldBe(100);
    }

    [Fact]
    public void SortBy_WithBuilder_ShouldSortDescending()
    {
        // Arrange
        var builder = new SortBuilder<Product>().On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.SortBy("Price", "desc", builder).ToList();

        // Assert
        result[0].Price.ShouldBe(100);
        result[1].Price.ShouldBe(75);
        result[2].Price.ShouldBe(50);
    }

    [Fact]
    public void SortBy_WithConfigureAction_ShouldApplySort()
    {
        // Act
        var result = Items
            .SortBy("Name", "asc", b => b.On(nameof(Product.Name), p => p.Name))
            .ToList();

        // Assert
        result[0].Name.ShouldBe("Bow");
        result[1].Name.ShouldBe("Shield");
        result[2].Name.ShouldBe("Sword");
    }

    [Fact]
    public void SortBy_WithNullSortField_ShouldReturnOriginalOrder()
    {
        // Arrange
        var builder = new SortBuilder<Product>().On(nameof(Product.Price), p => p.Price);

        // Act
        var result = Items.SortBy(null, null, builder).ToList();

        // Assert
        result.Count.ShouldBe(3);
    }
}
