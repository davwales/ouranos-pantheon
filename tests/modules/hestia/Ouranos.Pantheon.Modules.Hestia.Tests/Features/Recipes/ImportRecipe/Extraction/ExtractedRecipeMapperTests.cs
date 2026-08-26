using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe.Extraction;

public sealed class ExtractedRecipeMapperTests
{
    private static ExtractedRecipe ValidExtractedRecipe()
    {
        return new ExtractedRecipe(
            "Chocolate Cake",
            "A rich chocolate cake.",
            [
                new ExtractedIngredient(2m, "cup", "flour"),
                new ExtractedIngredient(3m, null, "eggs"),
            ],
            ["Mix everything.", "Bake at 350°F."]
        );
    }

    [Fact]
    public void TryMap_WhenValid_ShouldMapToDomainValues()
    {
        // Arrange
        var extracted = ValidExtractedRecipe();

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Title.ShouldBe("Chocolate Cake");
        mapped.Notes.ShouldBe("A rich chocolate cake.");
        mapped.Steps.ShouldBe([new Step("Mix everything."), new Step("Bake at 350°F.")]);
        mapped.Ingredients.ShouldBe([
            new Ingredient(2m, "cup", "flour"),
            new Ingredient(3m, "whole", "eggs"),
        ]);
    }

    [Fact]
    public void TryMap_WhenTitleIsBlank_ShouldReturnNull()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Title = "   ",
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryMap_WhenStepsAreBlank_ShouldReturnNull()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Steps = ["  ", ""],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryMap_WhenIngredientsAreBlank_ShouldReturnNull()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Ingredients = [new ExtractedIngredient(null, null, "  ")],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void TryMap_WhenStepIsBlank_ShouldSkipIt()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Steps = ["Mix.", "   ", "Bake."],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Steps.ShouldBe([new Step("Mix."), new Step("Bake.")]);
    }

    [Fact]
    public void TryMap_WhenIngredientNameIsBlank_ShouldSkipIt()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Ingredients =
            [
                new ExtractedIngredient(2m, "cup", "flour"),
                new ExtractedIngredient(1m, null, "  "),
            ],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Ingredients.ShouldBe([new Ingredient(2m, "cup", "flour")]);
    }

    [Fact]
    public void TryMap_WhenQuantityIsNull_ShouldDefaultToZero()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Ingredients = [new ExtractedIngredient(null, null, "salt")],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Ingredients.ShouldBe([new Ingredient(0m, "whole", "salt")]);
    }

    [Fact]
    public void TryMap_WhenQuantityIsNegative_ShouldClampToZero()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Ingredients = [new ExtractedIngredient(-1m, "cup", "flour")],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Ingredients.ShouldBe([new Ingredient(0m, "cup", "flour")]);
    }

    [Fact]
    public void TryMap_WhenUnitIsBlank_ShouldDefaultToWhole()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Ingredients = [new ExtractedIngredient(3m, "  ", "eggs")],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Ingredients.ShouldBe([new Ingredient(3m, "whole", "eggs")]);
    }

    [Fact]
    public void TryMap_WhenUnitHasMixedCase_ShouldLowercaseIt()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Ingredients = [new ExtractedIngredient(1m, "Cup", "flour")],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Ingredients.ShouldBe([new Ingredient(1m, "cup", "flour")]);
    }

    [Fact]
    public void TryMap_WhenTitleExceedsMaxLength_ShouldTruncate()
    {
        // Arrange
        var longTitle = new string('a', 250);
        var extracted = ValidExtractedRecipe() with { Title = longTitle };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Title.ShouldBe(new string('a', 200));
    }

    [Fact]
    public void TryMap_WhenStepExceedsMaxLength_ShouldTruncate()
    {
        // Arrange
        var longStep = new string('b', 2_500);
        var extracted = ValidExtractedRecipe() with { Steps = [longStep] };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Steps.ShouldBe([new Step(new string('b', 2_000))]);
    }

    [Fact]
    public void TryMap_WhenMoreThanMaxItems_ShouldCapLists()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Steps = [.. Enumerable.Range(0, 150).Select(i => $"Step {i}")],
            Ingredients =
            [
                .. Enumerable
                    .Range(0, 150)
                    .Select(i => new ExtractedIngredient(1m, "cup", $"flour {i}")),
            ],
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Steps.Count.ShouldBe(100);
        mapped.Ingredients.Count.ShouldBe(100);
    }

    [Fact]
    public void TryMap_WhenDescriptionIsNull_ShouldNotesBeEmpty()
    {
        // Arrange
        var extracted = ValidExtractedRecipe() with
        {
            Description = null,
        };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Notes.ShouldBe(string.Empty);
    }

    [Fact]
    public void TryMap_WhenDescriptionExceedsMaxLength_ShouldTruncate()
    {
        // Arrange
        var longDescription = new string('c', 12_000);
        var extracted = ValidExtractedRecipe() with { Description = longDescription };

        // Act
        var result = ExtractedRecipeMapper.TryMap(extracted);

        // Assert
        var mapped = result.ShouldBeOfType<MappedRecipe>();
        mapped.Notes.ShouldBe(new string('c', 10_000));
    }
}
