using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.GetShoppingList;

public sealed class GetShoppingListHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IQuerySession _session = Substitute.For<IQuerySession>();
    private readonly GetShoppingListHandler _handler;

    public GetShoppingListHandlerTests()
    {
        _store.QuerySession().Returns(_session);
        _handler = new GetShoppingListHandler(
            Substitute.For<ILogger<GetShoppingListHandler>>(),
            _store
        );
    }

    private static Recipe BuildRecipe(Guid id, params Ingredient[] ingredients)
    {
        return Recipe
            .Create(
                id,
                "Recipe " + id,
                null,
                [new Step("Do something.")],
                [.. ingredients],
                string.Empty
            )
            .State;
    }

    private static ShoppingList BuildList(
        List<Id<Recipe>> recipeIds,
        List<ManualItem>? manualItems = null,
        List<string>? checkedItemIds = null
    )
    {
        return new ShoppingList
        {
            RecipeIds = recipeIds,
            ManualItems = manualItems ?? [],
            CheckedItemIds = checkedItemIds ?? [],
        };
    }

    [Fact]
    public async Task Handle_WhenListNotFound_ShouldReturnEmptyResponse()
    {
        // Arrange
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(null));

        // Act
        var result = await _handler.Handle(new GetShoppingListInput(), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.RecipeIds.ShouldBeEmpty();
        result.ConsolidatedIngredients.ShouldBeEmpty();
        result.ManualItems.ShouldBeEmpty();
        result.CheckedItemIds.ShouldBeEmpty();
        await _session
            .DidNotReceive()
            .LoadManyAsync<Recipe>(Arg.Any<CancellationToken>(), Arg.Any<Guid[]>());
    }

    [Fact]
    public async Task Handle_WhenDuplicateNameAndUnitAcrossRecipes_ShouldSumQuantities()
    {
        // Arrange
        var recipeId1 = Guid.NewGuid();
        var recipeId2 = Guid.NewGuid();
        var list = BuildList([
            new Id<Recipe>(recipeId1.ToString()),
            new Id<Recipe>(recipeId2.ToString()),
        ]);

        var recipe1 = BuildRecipe(
            recipeId1,
            new Ingredient(300m, "g", "Flour"),
            new Ingredient(2m, "tbsp", "Butter")
        );
        var recipe2 = BuildRecipe(
            recipeId2,
            new Ingredient(500m, "g", "flour"),
            new Ingredient(100m, "g", "Butter")
        );

        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));
        _session
            .LoadManyAsync<Recipe>(Arg.Any<CancellationToken>(), Arg.Any<Guid[]>())
            .Returns(Task.FromResult<IReadOnlyList<Recipe>>([recipe1, recipe2]));

        // Act
        var result = await _handler.Handle(new GetShoppingListInput(), CancellationToken.None);

        // Assert
        result.ConsolidatedIngredients.Count.ShouldBe(3);
        var lines = result.ConsolidatedIngredients;
        lines[0].Name.ShouldBe("Butter");
        lines[0].Unit.ShouldBe("g");
        lines[0].Quantity.ShouldBe(100m);
        lines[1].Name.ShouldBe("Butter");
        lines[1].Unit.ShouldBe("tbsp");
        lines[1].Quantity.ShouldBe(2m);
        lines[2].Name.ShouldBe("Flour");
        lines[2].Unit.ShouldBe("g");
        lines[2].Quantity.ShouldBe(800m);
        lines[0].Id.ShouldBe("recipe:butter|g");
        lines[1].Id.ShouldBe("recipe:butter|tbsp");
        lines[2].Id.ShouldBe("recipe:flour|g");
    }

    [Fact]
    public async Task Handle_WhenConflictingUnitsForSameName_ShouldStaySeparateLines()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var list = BuildList([new Id<Recipe>(recipeId.ToString())]);
        var recipe = BuildRecipe(
            recipeId,
            new Ingredient(2m, "tbsp", "Butter"),
            new Ingredient(100m, "g", "Butter")
        );

        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));
        _session
            .LoadManyAsync<Recipe>(Arg.Any<CancellationToken>(), Arg.Any<Guid[]>())
            .Returns(Task.FromResult<IReadOnlyList<Recipe>>([recipe]));

        // Act
        var result = await _handler.Handle(new GetShoppingListInput(), CancellationToken.None);

        // Assert
        result.ConsolidatedIngredients.Count.ShouldBe(2);
        result.ConsolidatedIngredients.ShouldContain(c => c.Unit == "tbsp" && c.Quantity == 2m);
        result.ConsolidatedIngredients.ShouldContain(c => c.Unit == "g" && c.Quantity == 100m);
    }

    [Fact]
    public async Task Handle_WhenCheckedItemIdsContainStaleEntries_ShouldPruneToValidKeys()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var manualItemId = Guid.NewGuid();
        var staleManualId = Guid.NewGuid();
        var list = BuildList(
            [new Id<Recipe>(recipeId.ToString())],
            [new ManualItem(manualItemId, "Salt")],
            [
                "recipe:flour|g",
                ShoppingListNormalizer.ManualLineKey(manualItemId),
                ShoppingListNormalizer.ManualLineKey(staleManualId),
                "recipe:stale|stale",
            ]
        );
        var recipe = BuildRecipe(recipeId, new Ingredient(800m, "g", "Flour"));

        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));
        _session
            .LoadManyAsync<Recipe>(Arg.Any<CancellationToken>(), Arg.Any<Guid[]>())
            .Returns(Task.FromResult<IReadOnlyList<Recipe>>([recipe]));

        // Act
        var result = await _handler.Handle(new GetShoppingListInput(), CancellationToken.None);

        // Assert
        result.CheckedItemIds.Count.ShouldBe(2);
        result.CheckedItemIds.ShouldContain("recipe:flour|g");
        result.CheckedItemIds.ShouldContain(ShoppingListNormalizer.ManualLineKey(manualItemId));
        result.CheckedItemIds.ShouldNotContain(ShoppingListNormalizer.ManualLineKey(staleManualId));
        result.CheckedItemIds.ShouldNotContain("recipe:stale|stale");
    }

    [Fact]
    public async Task Handle_WhenRecipeMissingFromStore_ShouldSkipNullRecipes()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        var validId = Guid.NewGuid();
        var list = BuildList([
            new Id<Recipe>(missingId.ToString()),
            new Id<Recipe>(validId.ToString()),
        ]);
        var validRecipe = BuildRecipe(validId, new Ingredient(1m, "g", "Sugar"));

        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));
        _session
            .LoadManyAsync<Recipe>(Arg.Any<CancellationToken>(), Arg.Any<Guid[]>())
            .Returns(Task.FromResult<IReadOnlyList<Recipe>>([validRecipe]));

        // Act
        var result = await _handler.Handle(new GetShoppingListInput(), CancellationToken.None);

        // Assert
        result.ConsolidatedIngredients.Count.ShouldBe(1);
        result.ConsolidatedIngredients[0].Name.ShouldBe("Sugar");
    }

    [Fact]
    public async Task Handle_WhenManualItemsPresent_ShouldReturnThemAsResponses()
    {
        // Arrange
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var list = BuildList(
            [],
            [new ManualItem(firstId, "Olive oil"), new ManualItem(secondId, "Bread")]
        );

        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var result = await _handler.Handle(new GetShoppingListInput(), CancellationToken.None);

        // Assert
        result.ManualItems.Count.ShouldBe(2);
        result.ManualItems[0].Id.ShouldBe(firstId);
        result.ManualItems[0].Text.ShouldBe("Olive oil");
        result.ManualItems[1].Id.ShouldBe(secondId);
        result.ManualItems[1].Text.ShouldBe("Bread");
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var ct = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(new GetShoppingListInput(), ct);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
