using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Common.Filtering;

public sealed class FilterExpressionBuilderTests
{
    private sealed record Item(string Name, int Price, bool IsActive, string? Category);
    private sealed record ItemWithId(Id<Item> Id, string Name);

    private readonly IQueryable<Item> _items = new Item[]
    {
        new("Sword", 100, true, "Weapon"),
        new("Shield", 50, false, "Armor"),
        new("Bow", 75, true, "Weapon"),
    }.AsQueryable();

    [Fact]
    public void FilterBy_WhenStartsWithFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Name), x => x.Name);

        // Act
        var result = _items.FilterBy(["Name:startswith:S"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(i => i.Name.StartsWith("S"));
    }

    [Fact]
    public void FilterBy_WhenEndsWithFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Name), x => x.Name);

        // Act
        var result = _items.FilterBy(["Name:endswith:ow"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Bow");
    }

    [Fact]
    public void FilterBy_WhenLtFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Price), x => x.Price);

        // Act
        var result = _items.FilterBy(["Price:lt:75"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Shield");
    }

    [Fact]
    public void FilterBy_WhenLteFilter_ShouldReturnMatchingItems()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Price), x => x.Price);

        // Act
        var result = _items.FilterBy(["Price:lte:75"], builder).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void ConvertValue_WhenIdType_ShouldCreateIdInstance()
    {
        // Arrange
        var items = new ItemWithId[]
        {
            new(new Id<Item>("id-1"), "Alpha"),
            new(new Id<Item>("id-2"), "Beta"),
        }.AsQueryable();

        var builder = new FilterBuilder<ItemWithId>().On(nameof(ItemWithId.Id), x => x.Id);

        // Act
        var result = items.FilterBy(["Id:eq:id-1"], builder).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Alpha");
    }

    [Fact]
    public void FilterBy_WhenNullCheckOnNonNullableProperty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Price), x => x.Price);

        // Act
        var act = () => _items.FilterBy(["Price:null"], builder).ToList();

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void FilterBy_WhenStringOpOnNonStringProperty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new FilterBuilder<Item>().On(nameof(Item.Price), x => x.Price);

        // Act
        var act = () => _items.FilterBy(["Price:like:foo"], builder).ToList();

        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }

    private sealed record TypedItem(long LongVal, float FloatVal, double DoubleVal, Guid GuidVal, DateTime DateVal, DateTimeOffset DateOffsetVal);

    [Theory]
    [InlineData("LongVal:eq:42")]
    [InlineData("FloatVal:gt:0")]
    [InlineData("DoubleVal:lt:999")]
    [InlineData("GuidVal:eq:00000000-0000-0000-0000-000000000001")]
    [InlineData("DateVal:lt:2099-01-01")]
    [InlineData("DateOffsetVal:lt:2099-01-01")]
    public void FilterBy_WhenVariousFieldTypes_ShouldApplyFilterWithoutThrowing(string filterExpression)
    {
        // Arrange
        var guid = new Guid("00000000-0000-0000-0000-000000000001");
        var now = DateTime.UtcNow;
        var items = new TypedItem[]
        {
            new(42L, 1.5f, 2.5, guid, now, DateTimeOffset.UtcNow)
        }.AsQueryable();

        var builder = new FilterBuilder<TypedItem>()
            .On(nameof(TypedItem.LongVal), x => x.LongVal)
            .On(nameof(TypedItem.FloatVal), x => x.FloatVal)
            .On(nameof(TypedItem.DoubleVal), x => x.DoubleVal)
            .On(nameof(TypedItem.GuidVal), x => x.GuidVal)
            .On(nameof(TypedItem.DateVal), x => x.DateVal)
            .On(nameof(TypedItem.DateOffsetVal), x => x.DateOffsetVal);

        // Act
        var result = items.FilterBy([filterExpression], builder).ToList();

        // Assert
        result.ShouldNotBeNull();
    }
}
