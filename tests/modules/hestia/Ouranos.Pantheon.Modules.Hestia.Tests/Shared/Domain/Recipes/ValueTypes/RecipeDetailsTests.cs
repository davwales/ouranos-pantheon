using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared.Domain.Recipes.ValueTypes;

public sealed class RecipeDetailsTests
{
    [Fact]
    public void Constructor_WhenValuesValid_ShouldTrimAndStore()
    {
        // Act
        var details = new RecipeDetails(" 4 servings ", 10, 20, 30, " https://example.com/a.jpg ");

        // Assert
        details.Yield.ShouldBe("4 servings");
        details.PrepMinutes.ShouldBe(10);
        details.CookMinutes.ShouldBe(20);
        details.TotalMinutes.ShouldBe(30);
        details.ImageUrl.ShouldBe("https://example.com/a.jpg");
    }

    [Fact]
    public void Constructor_WhenTextIsBlank_ShouldStoreNull()
    {
        // Act
        var details = new RecipeDetails("  ", null, null, null, "");

        // Assert
        details.ShouldBe(RecipeDetails.Empty);
    }

    [Fact]
    public void Constructor_WhenYieldExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () =>
            new RecipeDetails(
                new string('x', RecipeDetails.MaxYieldLength + 1),
                null,
                null,
                null,
                null
            );

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(RecipeDetails.MaxMinutes + 1)]
    public void Constructor_WhenMinutesOutOfRange_ShouldThrowArgumentOutOfRangeException(
        int minutes
    )
    {
        // Assert
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new RecipeDetails(null, minutes, null, null, null)
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new RecipeDetails(null, null, minutes, null, null)
        );
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new RecipeDetails(null, null, null, minutes, null)
        );
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("/relative.jpg")]
    [InlineData("ftp://example.com/a.jpg")]
    [InlineData("javascript:alert(1)")]
    public void Constructor_WhenImageUrlIsNotAbsoluteHttp_ShouldThrowArgumentException(string url)
    {
        // Act
        var act = () => new RecipeDetails(null, null, null, null, url);

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenImageUrlExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var url = "https://example.com/" + new string('a', RecipeDetails.MaxImageUrlLength);

        // Act
        var act = () => new RecipeDetails(null, null, null, null, url);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }
}
