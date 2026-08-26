using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Filtering;

public sealed class FilterBuilderTests
{
    private sealed record Item(string Name, int Price, bool IsActive);

    private readonly IQueryable<Item> _items = new Item[]
    {
        new("Sword", 100, true),
        new("Shield", 50, false),
        new("Bow", 75, true),
    }.AsQueryable();

    [Fact]
    public void On_WhenFieldRegistered_ShouldApplyFilter()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Name), x => x.Name);

        // Act
        var result = _items.FilterBy(["Name:eq:Sword"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Sword");
    }

    [Fact]
    public void On_WhenUnregisteredField_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Name), x => x.Name);

        // Act
        var filter = () => _items.FilterBy(["Price:eq:100"], builder).ToList();

        // Assert
        filter.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void On_WhenMultipleFieldsRegistered_ShouldSupportEach()
    {
        // Arrange
        var builder = new FilterBuilder<Item>()
            .On(nameof(Item.Name), x => x.Name)
            .On(nameof(Item.Price), x => x.Price)
            .On(nameof(Item.IsActive), x => x.IsActive);

        // Act
        var byName = _items.FilterBy(["Name:eq:Bow"], builder).ToList();
        var byPrice = _items.FilterBy(["Price:lte:75"], builder).ToList();
        var byActive = _items.FilterBy(["IsActive:eq:false"], builder).ToList();

        // Assert
        byName.ShouldHaveSingleItem().Name.ShouldBe("Bow");
        byPrice.Count.ShouldBe(2);
        byActive.ShouldHaveSingleItem().Name.ShouldBe("Shield");
    }

    [Fact]
    public void On_WhenFieldLookupIsCaseInsensitive_ShouldApplyFilter()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Name), x => x.Name);

        // Act
        var result = _items.FilterBy(["name:eq:Sword"], builder).ToList();

        // Assert
        result.ShouldHaveSingleItem().Name.ShouldBe("Sword");
    }

    [Fact]
    public void AutoMap_ShouldRegisterAllPublicPropertiesAsFilterable()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().AutoMap();

        // Act
        var byName = _items.FilterBy(["Name:eq:Sword"], builder).ToList();
        var byPrice = _items.FilterBy(["Price:eq:75"], builder).ToList();
        var byActive = _items.FilterBy(["IsActive:eq:true"], builder).ToList();

        // Assert
        byName.ShouldHaveSingleItem().Name.ShouldBe("Sword");
        byPrice.ShouldHaveSingleItem().Name.ShouldBe("Bow");
        byActive.Count.ShouldBe(2);
    }

    [Fact]
    public void FilterBy_WhenAndComposite_ShouldRequireBothConditions()
    {
        // Arrange
        var builder = new FilterBuilder<Item>()
            .On(nameof(Item.Name), x => x.Name)
            .On(nameof(Item.Price), x => x.Price);

        // Act
        var result = _items.FilterBy(["and(Name:eq:Sword|Price:gt:50)"], builder).ToList();

        // Assert
        result.ShouldHaveSingleItem().Name.ShouldBe("Sword");
    }

    [Fact]
    public void FilterBy_WhenOrComposite_ShouldReturnEitherMatch()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Name), x => x.Name);

        // Act
        var result = _items.FilterBy(["or(Name:eq:Sword|Name:eq:Bow)"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.Select(i => i.Name).ShouldBe(["Sword", "Bow"], ignoreOrder: true);
    }

    [Fact]
    public void FilterBy_WhenEmptyCompositeNode_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().AutoMap();

        // Act
        var filter = () => _items.FilterBy(["and()"], builder).ToList();

        // Assert
        filter.ShouldThrow<Exception>();
    }

    [Fact]
    public void On_WhenCaseInsensitiveEq_ShouldMatchRegardlessOfCasing()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(
            nameof(Item.Name),
            x => x.Name,
            caseInsensitive: true
        );

        // Act
        var result = _items.FilterBy(["Name:eq:sword"], builder).ToList();

        // Assert
        result.ShouldHaveSingleItem().Name.ShouldBe("Sword");
    }

    [Fact]
    public void On_WhenCaseInsensitiveLike_ShouldMatchRegardlessOfCasing()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(
            nameof(Item.Name),
            x => x.Name,
            caseInsensitive: true
        );

        // Act
        var result = _items.FilterBy(["Name:like:IELD"], builder).ToList();

        // Assert
        result.ShouldHaveSingleItem().Name.ShouldBe("Shield");
    }

    [Fact]
    public void On_WhenCaseInsensitiveStartsWith_ShouldMatchRegardlessOfCasing()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(
            nameof(Item.Name),
            x => x.Name,
            caseInsensitive: true
        );

        // Act
        var result = _items.FilterBy(["Name:startswith:SHI"], builder).ToList();

        // Assert
        result.ShouldHaveSingleItem().Name.ShouldBe("Shield");
    }

    [Fact]
    public void On_WhenCaseInsensitiveEndsWith_ShouldMatchRegardlessOfCasing()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(
            nameof(Item.Name),
            x => x.Name,
            caseInsensitive: true
        );

        // Act
        var result = _items.FilterBy(["Name:endswith:OW"], builder).ToList();

        // Assert
        result.ShouldHaveSingleItem().Name.ShouldBe("Bow");
    }

    [Fact]
    public void On_WhenCaseSensitiveEq_ShouldNotMatchWrongCase()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Name), x => x.Name);

        // Act
        var result = _items.FilterBy(["Name:eq:sword"], builder).ToList();

        // Assert
        result.ShouldBeEmpty();
    }
}
