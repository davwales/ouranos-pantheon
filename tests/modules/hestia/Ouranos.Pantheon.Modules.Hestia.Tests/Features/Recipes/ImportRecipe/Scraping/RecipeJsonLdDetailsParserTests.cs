using System.Text.Json;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe.Scraping;

public sealed class RecipeJsonLdDetailsParserTests
{
    private static RecipeDetails Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return RecipeJsonLdDetailsParser.Parse(document.RootElement);
    }

    [Fact]
    public void Parse_WhenNoDetailsPresent_ShouldReturnEmpty()
    {
        // Act
        var result = Parse("""{ "@type": "Recipe" }""");

        // Assert
        result.ShouldBe(RecipeDetails.Empty);
    }

    [Theory]
    [InlineData("\"4 servings\"", "4 servings")]
    [InlineData("""12""", "12")]
    [InlineData("""["24", "24 cookies"]""", "24 cookies")]
    [InlineData("""["6"]""", "6")]
    [InlineData("\"   \"", null)]
    [InlineData("""{ "value": 4 }""", null)]
    public void Parse_WhenYieldPresent_ShouldPreferDescriptiveValue(string yield, string? expected)
    {
        // Act
        var result = Parse($$"""{ "recipeYield": {{yield}} }""");

        // Assert
        result.Yield.ShouldBe(expected);
    }

    [Fact]
    public void Parse_WhenYieldIsTooLong_ShouldTruncate()
    {
        // Act
        var result = Parse($$"""{ "recipeYield": "{{new string('x', 150)}}" }""");

        // Assert
        result.Yield!.Length.ShouldBe(RecipeDetails.MaxYieldLength);
    }

    [Theory]
    [InlineData("PT15M", 15)]
    [InlineData("PT1H30M", 90)]
    [InlineData("P0DT2H", 120)]
    [InlineData("PT0H45M", 45)]
    [InlineData("PT0M", null)]
    [InlineData("P30D", null)]
    [InlineData("15 minutes", null)]
    [InlineData("", null)]
    public void Parse_WhenTimesPresent_ShouldConvertIsoDurationsToMinutes(
        string duration,
        int? expected
    )
    {
        // Act
        var result = Parse(
            $$"""{ "prepTime": "{{duration}}", "cookTime": "{{duration}}", "totalTime": "{{duration}}" }"""
        );

        // Assert
        result.PrepMinutes.ShouldBe(expected);
        result.CookMinutes.ShouldBe(expected);
        result.TotalMinutes.ShouldBe(expected);
    }

    [Theory]
    [InlineData("\"https://example.com/a.jpg\"", "https://example.com/a.jpg")]
    [InlineData(
        """["https://example.com/a.jpg", "https://example.com/b.jpg"]""",
        "https://example.com/a.jpg"
    )]
    [InlineData(
        """{ "@type": "ImageObject", "url": "https://example.com/a.jpg" }""",
        "https://example.com/a.jpg"
    )]
    [InlineData(
        """[{ "url": "/relative.jpg" }, { "url": "https://example.com/b.jpg" }]""",
        "https://example.com/b.jpg"
    )]
    [InlineData("\"javascript:alert(1)\"", null)]
    [InlineData("\"/relative.jpg\"", null)]
    public void Parse_WhenImagePresent_ShouldReturnFirstAbsoluteHttpUrl(
        string image,
        string? expected
    )
    {
        // Act
        var result = Parse($$"""{ "image": {{image}} }""");

        // Assert
        result.ImageUrl.ShouldBe(expected);
    }
}
