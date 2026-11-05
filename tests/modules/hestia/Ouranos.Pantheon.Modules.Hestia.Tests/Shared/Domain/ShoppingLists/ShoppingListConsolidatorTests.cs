using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared.Domain.ShoppingLists;

public sealed class ShoppingListConsolidatorTests
{
    private static Recipe BuildRecipe(params Ingredient[] ingredients)
    {
        return Recipe
            .Create(
                Guid.NewGuid(),
                "Recipe",
                null,
                [new Step("Do something.")],
                [.. ingredients],
                string.Empty
            )
            .State;
    }

    [Fact]
    public void Consolidate_WhenUnitSpellingsDiffer_ShouldMergeIntoCanonicalUnit()
    {
        // Arrange
        var first = BuildRecipe(new Ingredient(1m, "tablespoon", "Sugar"));
        var second = BuildRecipe(new Ingredient(2m, "Tablespoons", "sugar"));

        // Act
        var lines = ShoppingListConsolidator.Consolidate([first, second]);

        // Assert
        var line = lines.ShouldHaveSingleItem();
        line.Name.ShouldBe("Sugar");
        line.Unit.ShouldBe("tbsp");
        line.Quantity.ShouldBe(3m);
        line.Id.ShouldBe("recipe:sugar|volume");
    }

    [Fact]
    public void Consolidate_WhenCompatibleUnitsMix_ShouldConvertToLargestUnitUsed()
    {
        // Arrange
        var first = BuildRecipe(new Ingredient(3m, "tsp", "Salt"));
        var second = BuildRecipe(new Ingredient(1m, "tbsp", "Salt"));

        // Act
        var lines = ShoppingListConsolidator.Consolidate([first, second]);

        // Assert
        var line = lines.ShouldHaveSingleItem();
        line.Unit.ShouldBe("tbsp");
        line.Quantity.ShouldBe(2m);
    }

    [Fact]
    public void Consolidate_WhenMassUnitsMix_ShouldConvertAcrossSystems()
    {
        // Arrange
        var first = BuildRecipe(new Ingredient(1m, "lb", "Butter"));
        var second = BuildRecipe(new Ingredient(8m, "oz", "Butter"));

        // Act
        var lines = ShoppingListConsolidator.Consolidate([first, second]);

        // Assert
        var line = lines.ShouldHaveSingleItem();
        line.Unit.ShouldBe("lb");
        line.Quantity.ShouldBe(1.5m);
    }

    [Fact]
    public void Consolidate_WhenNamesDifferOnlyByPlural_ShouldMerge()
    {
        // Arrange
        var first = BuildRecipe(new Ingredient(2m, "whole", "Eggs"));
        var second = BuildRecipe(new Ingredient(1m, "whole", "egg"));

        // Act
        var lines = ShoppingListConsolidator.Consolidate([first, second]);

        // Assert
        var line = lines.ShouldHaveSingleItem();
        line.Name.ShouldBe("Eggs");
        line.Quantity.ShouldBe(3m);
        line.NameKey.ShouldBe("egg");
    }

    [Fact]
    public void Consolidate_WhenVolumeAndMassOfSameIngredient_ShouldKeepSeparateLines()
    {
        // Arrange
        var recipe = BuildRecipe(
            new Ingredient(2m, "cup", "Flour"),
            new Ingredient(100m, "g", "Flour")
        );

        // Act
        var lines = ShoppingListConsolidator.Consolidate([recipe]);

        // Assert
        lines
            .Select(l => l.Id)
            .ShouldBe(["recipe:flour|volume", "recipe:flour|mass"], ignoreOrder: true);
    }

    [Fact]
    public void Consolidate_WhenUnitDoesNotConvert_ShouldMergeOnlyMatchingUnits()
    {
        // Arrange
        var first = BuildRecipe(new Ingredient(2m, "cloves", "Garlic"));
        var second = BuildRecipe(
            new Ingredient(1m, "clove", "garlic"),
            new Ingredient(1m, "head", "Garlic")
        );

        // Act
        var lines = ShoppingListConsolidator.Consolidate([first, second]);

        // Assert
        lines.Count.ShouldBe(2);
        var cloves = lines.Single(l => l.Id == "recipe:garlic|clove");
        cloves.Quantity.ShouldBe(3m);
        cloves.Unit.ShouldBe("cloves");
    }

    [Fact]
    public void Consolidate_WhenIngredientUnquantified_ShouldKeepItAsIsAndApart()
    {
        // Arrange
        var first = BuildRecipe(
            new Ingredient(0m, "to taste", "Salt"),
            new Ingredient(1m, "tsp", "Salt")
        );
        var second = BuildRecipe(new Ingredient(0m, "as needed", "salt"));

        // Act
        var lines = ShoppingListConsolidator.Consolidate([first, second]);

        // Assert
        lines.Count.ShouldBe(2);
        var unquantified = lines.Single(l => l.Id == "recipe:salt|unquantified");
        unquantified.Quantity.ShouldBe(0m);
        unquantified.Unit.ShouldBe("to taste");
    }

    [Fact]
    public void Consolidate_WhenIngredientOptional_ShouldLeaveItOff()
    {
        // Arrange
        var recipe = BuildRecipe(
            new Ingredient(1m, "cup", "Rice"),
            new Ingredient(1m, "bunch", "Parsley", isOptional: true)
        );

        // Act
        var lines = ShoppingListConsolidator.Consolidate([recipe]);

        // Assert
        lines.ShouldHaveSingleItem().Name.ShouldBe("Rice");
    }

    [Fact]
    public void LineIds_WhenRecipeEdited_ShouldKeepIdsForUnitAndPluralChanges()
    {
        // Arrange
        var before = BuildRecipe(new Ingredient(2m, "tablespoons", "Onions"));
        var after = BuildRecipe(new Ingredient(3m, "tbsp", "onion"));

        // Act
        var beforeIds = ShoppingListConsolidator.LineIds(before);
        var afterIds = ShoppingListConsolidator.LineIds(after);

        // Assert
        afterIds.ShouldBe(beforeIds);
    }
}
