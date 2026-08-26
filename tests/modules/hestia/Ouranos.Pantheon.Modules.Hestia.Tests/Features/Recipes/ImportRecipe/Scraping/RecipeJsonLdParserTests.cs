using System.Text.Json;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe.Scraping;

public sealed class RecipeJsonLdParserTests
{
    private static string JsonLdHtml(string json)
    {
        return $"""<html><head><script type="application/ld+json">{json}</script></head><body></body></html>""";
    }

    private static string NameFromRaw(string rawJson)
    {
        using var document = JsonDocument.Parse(rawJson);
        return document.RootElement.GetProperty("name").GetString()!;
    }

    [Fact]
    public void TryExtractRecipe_WhenSingleRecipeBlock_ShouldReturnRawRecipeJson()
    {
        // Arrange
        var html = JsonLdHtml(
            """
            {
              "@context": "https://schema.org",
              "@type": "Recipe",
              "name": "Chocolate Cake",
              "description": "A rich chocolate cake.",
              "recipeIngredient": ["2 cups flour", "1 cup sugar"],
              "recipeInstructions": [
                { "@type": "HowToStep", "text": "Mix everything." },
                { "@type": "HowToStep", "text": "Bake at 350°F." }
              ]
            }
            """
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Chocolate Cake");
        NameFromRaw(recipe.RawJson).ShouldBe("Chocolate Cake");
        recipe.RawJson.ShouldContain("recipeIngredient");
        recipe.RawJson.ShouldContain("recipeInstructions");
    }

    [Fact]
    public void TryExtractRecipe_WhenRecipeInsideGraph_ShouldExtractRecipe()
    {
        // Arrange
        var html = JsonLdHtml(
            """
            {
              "@context": "https://schema.org",
              "@graph": [
                { "@type": "WebPage", "name": "Page" },
                { "@type": "Recipe", "name": "Graph Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }
              ]
            }
            """
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Graph Cake");
        NameFromRaw(recipe.RawJson).ShouldBe("Graph Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenRecipeInsideNestedGraph_ShouldExtractRecipe()
    {
        // Arrange
        var html = JsonLdHtml(
            """
            {
              "@context": "https://schema.org",
              "@graph": [
                {
                  "@type": "ItemList",
                  "@graph": [
                    { "@type": "Recipe", "name": "Nested Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }
                  ]
                }
              ]
            }
            """
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Nested Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenRootIsArray_ShouldExtractRecipe()
    {
        // Arrange
        var html = JsonLdHtml(
            """
            [
              { "@type": "WebPage", "name": "Page" },
              { "@type": "Recipe", "name": "Array Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }
            ]
            """
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Array Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenTypeIsArrayContainingRecipe_ShouldExtractRecipe()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": ["Article", "Recipe"], "name": "Typed Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Typed Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenMultipleBlocks_ShouldUseFirstRecipe()
    {
        // Arrange
        var html =
            JsonLdHtml("""{ "@type": "WebPage", "name": "Page" }""")
            + JsonLdHtml(
                """{ "@type": "Recipe", "name": "Second Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }"""
            );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Second Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenFirstBlockHasInvalidJson_ShouldTryNextBlock()
    {
        // Arrange
        var html =
            JsonLdHtml("""{ "@type": "Recipe", "name": "Broken", }}""")
            + JsonLdHtml(
                """{ "@type": "Recipe", "name": "Valid Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }"""
            );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Valid Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenJsonHasTrailingCommas_ShouldStillParse()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "Loose Cake", "recipeIngredient": ["1 egg",], "recipeInstructions": ["Bake."], }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Loose Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenJsonStartsWithBom_ShouldStillParse()
    {
        // Arrange
        var html = JsonLdHtml(
            "\uFEFF"
                + """{ "@type": "Recipe", "name": "Bom Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Bom Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenPageHasNoRecipeMetadata_ShouldReturnNull()
    {
        // Arrange
        var html = """<html><body><p>Just a blog post.</p></body></html>""";

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryExtractRecipe_WhenNoRecipeTypePresent_ShouldReturnNull()
    {
        // Arrange
        var html = JsonLdHtml("""{ "@type": "WebPage", "name": "Page" }""");

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryExtractRecipe_WhenNameMissing_ShouldReturnNull()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryExtractRecipe_WhenIngredientsMissing_ShouldReturnNull()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "No List", "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryExtractRecipe_WhenInstructionsMissing_ShouldReturnNull()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "No Steps", "recipeIngredient": ["1 egg"] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryExtractRecipe_WhenInstructionsAreSingleObject_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "Object Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": { "@type": "HowToStep", "text": "Bake." } }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Object Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenInstructionsAreSingleString_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "Split Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": "Mix everything.\nBake at 350°F." }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Split Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenInstructionsUseHowToSection_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """
            {
              "@type": "Recipe",
              "name": "Section Cake",
              "recipeIngredient": ["1 egg"],
              "recipeInstructions": [
                {
                  "@type": "HowToSection",
                  "name": "Crust",
                  "itemListElement": [
                    { "@type": "HowToStep", "text": "Mix crust." }
                  ]
                },
                { "@type": "HowToStep", "text": "Assemble." }
              ]
            }
            """
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Section Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenNameContainsEntitiesAndTags_ShouldPreserveRawValue()
    {
        // Arrange
        var html = JsonLdHtml(
            """
            {
              "@type": "Recipe",
              "name": "Chocolate &amp; Vanilla <b>Cake</b>",
              "recipeIngredient": ["1 egg"],
              "recipeInstructions": [ { "@type": "HowToStep", "text": "<p>Mix.</p><p>Bake.</p>" } ]
            }
            """
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Chocolate &amp; Vanilla <b>Cake</b>");
        NameFromRaw(recipe.RawJson).ShouldBe("Chocolate &amp; Vanilla <b>Cake</b>");
    }

    [Fact]
    public void TryExtractRecipe_WhenScriptTagAttributesVary_ShouldStillMatch()
    {
        // Arrange
        var html =
            $"""<html><head><script data-test="1" type = 'application/ld+json'>{"""{"@type":"Recipe","name":"Attr Cake","recipeIngredient":["1 egg"],"recipeInstructions":["Bake."]}"""}</script></head></html>""";

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Attr Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenTypeIsLowercase_ShouldStillExtractRecipe()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "recipe", "name": "Lower Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Lower Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenOnlyIngredientsAliasPresent_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "Alias Cake", "ingredients": ["1 egg", "2 cups flour"], "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Alias Cake");
        recipe.RawJson.ShouldContain("\"ingredients\"");
    }

    [Fact]
    public void TryExtractRecipe_WhenIngredientsAreSingleString_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "String Cake", "recipeIngredient": "1 egg", "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("String Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenIngredientListContainsNonStrings_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "Tolerant Cake", "recipeIngredient": ["1 egg", { "@type": "Thing" }], "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Tolerant Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenInstructionsMixStringsAndSteps_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", "name": "Mixed Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Mix.", { "@type": "HowToStep", "text": "Bake." }] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Mixed Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenSectionContainsPlainStringSteps_ShouldStillValidate()
    {
        // Arrange
        var html = JsonLdHtml(
            """
            {
              "@type": "Recipe",
              "name": "String Section Cake",
              "recipeIngredient": ["1 egg"],
              "recipeInstructions": [
                { "@type": "HowToSection", "itemListElement": ["Mix.", "Bake."] }
              ]
            }
            """
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("String Section Cake");
    }

    [Fact]
    public void TryExtractRecipe_WhenJsonContainsComments_ShouldStillParse()
    {
        // Arrange
        var html = JsonLdHtml(
            """{ "@type": "Recipe", /* note */ "name": "Commented Cake", "recipeIngredient": ["1 egg"], "recipeInstructions": ["Bake."] }"""
        );

        // Act
        var result = RecipeJsonLdParser.TryExtractRecipe(html);

        // Assert
        var recipe = result.ShouldBeOfType<ScrapedJsonLdRecipe>();
        recipe.Title.ShouldBe("Commented Cake");
    }
}
