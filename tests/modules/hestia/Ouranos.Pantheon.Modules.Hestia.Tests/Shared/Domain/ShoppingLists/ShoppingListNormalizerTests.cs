using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared.Domain.ShoppingLists;

public sealed class ShoppingListNormalizerTests
{
    [Fact]
    public void Normalize_WhenLeadingAndTrailingWhitespace_ShouldTrimAndLowercase()
    {
        // Arrange
        var value = "  Flour  ";

        // Act
        var result = ShoppingListNormalizer.Normalize(value);

        // Assert
        result.ShouldBe("flour");
    }

    [Fact]
    public void Normalize_WhenInternalWhitespaceRuns_ShouldCollapseToSingleSpace()
    {
        // Arrange
        var value = "Olive   Oil\tSpray";

        // Act
        var result = ShoppingListNormalizer.Normalize(value);

        // Assert
        result.ShouldBe("olive oil spray");
    }

    [Fact]
    public void Normalize_WhenMixedCase_ShouldLowercaseInvariant()
    {
        // Arrange
        var value = "GraNuLaTeD SuGaR";

        // Act
        var result = ShoppingListNormalizer.Normalize(value);

        // Assert
        result.ShouldBe("granulated sugar");
    }

    [Fact]
    public void Normalize_WhenNull_ShouldReturnEmptyString()
    {
        // Arrange
        string? value = null;

        // Act
        var result = ShoppingListNormalizer.Normalize(value!);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void Normalize_WhenEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        var value = "";

        // Act
        var result = ShoppingListNormalizer.Normalize(value);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void RecipeLineKey_WhenCalled_ShouldCombineNormalizedNameAndUnit()
    {
        // Arrange
        const string name = "flour";
        const string unit = "g";

        // Act
        var result = ShoppingListNormalizer.RecipeLineKey(name, unit);

        // Assert
        result.ShouldBe("recipe:flour|g");
    }

    [Fact]
    public void ManualLineKey_WhenCalled_ShouldPrefixManualItemId()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = ShoppingListNormalizer.ManualLineKey(id);

        // Assert
        result.ShouldBe("manual:11111111-1111-1111-1111-111111111111");
    }
}
