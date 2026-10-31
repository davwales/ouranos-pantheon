using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ClearCheckedItems.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.ClearCheckedItems;

public sealed class ClearCheckedItemsHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly ClearCheckedItemsHandler _handler;

    public ClearCheckedItemsHandlerTests()
    {
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);
        _handler = new ClearCheckedItemsHandler(
            Substitute.For<ILogger<ClearCheckedItemsHandler>>(),
            _store
        );
    }

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

    private static Id<Recipe> IdOf(Recipe recipe)
    {
        return new Id<Recipe>(recipe.Id.ToString());
    }

    private void Given(ShoppingList list, params Recipe[] recipes)
    {
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));
        _session
            .LoadManyAsync<Recipe>(Arg.Any<CancellationToken>(), Arg.Any<Guid[]>())
            .Returns(Task.FromResult<IReadOnlyList<Recipe>>(recipes));
    }

    [Fact]
    public async Task Handle_WhenManualItemsChecked_ShouldRemoveOnlyThoseItems()
    {
        // Arrange
        var bought = new ManualItem(Guid.NewGuid(), "Milk");
        var needed = new ManualItem(Guid.NewGuid(), "Bread");
        var list = new ShoppingList
        {
            ManualItems = [bought, needed],
            CheckedItemIds = [ShoppingListNormalizer.ManualLineKey(bought.Id)],
        };
        Given(list);

        // Act
        var result = await _handler.Handle(new ClearCheckedItemsInput(), CancellationToken.None);

        // Assert
        result.RemovedManualItemIds.ShouldBe([bought.Id]);
        list.ManualItems.ShouldBe([needed]);
        list.CheckedItemIds.ShouldBeEmpty();
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRecipeFullyChecked_ShouldRemoveRecipe()
    {
        // Arrange
        var bought = BuildRecipe(
            new Ingredient(2m, "cup", "Flour"),
            new Ingredient(1m, "tsp", "Salt")
        );
        var list = new ShoppingList
        {
            RecipeIds = [IdOf(bought)],
            CheckedItemIds = ["recipe:flour|volume", "recipe:salt|volume"],
        };
        Given(list, bought);

        // Act
        var result = await _handler.Handle(new ClearCheckedItemsInput(), CancellationToken.None);

        // Assert
        result.RemovedRecipeIds.ShouldBe([IdOf(bought)]);
        list.RecipeIds.ShouldBeEmpty();
        list.CheckedItemIds.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRecipePartlyChecked_ShouldKeepRecipeAndItsChecks()
    {
        // Arrange
        var bought = BuildRecipe(new Ingredient(2m, "cup", "Flour"));
        var partial = BuildRecipe(
            new Ingredient(1m, "cup", "Flour"),
            new Ingredient(2m, "whole", "Eggs")
        );
        var list = new ShoppingList
        {
            RecipeIds = [IdOf(bought), IdOf(partial)],
            CheckedItemIds = ["recipe:flour|volume"],
        };
        Given(list, bought, partial);

        // Act
        var result = await _handler.Handle(new ClearCheckedItemsInput(), CancellationToken.None);

        // Assert
        result.RemovedRecipeIds.ShouldBe([IdOf(bought)]);
        list.RecipeIds.ShouldBe([IdOf(partial)]);
        list.CheckedItemIds.ShouldBe(["recipe:flour|volume"]);
    }

    [Fact]
    public async Task Handle_WhenNothingChecked_ShouldLeaveListUnchanged()
    {
        // Arrange
        var recipe = BuildRecipe(new Ingredient(2m, "cup", "Flour"));
        var item = new ManualItem(Guid.NewGuid(), "Milk");
        var list = new ShoppingList { RecipeIds = [IdOf(recipe)], ManualItems = [item] };
        Given(list, recipe);

        // Act
        var result = await _handler.Handle(new ClearCheckedItemsInput(), CancellationToken.None);

        // Assert
        result.RemovedManualItemIds.ShouldBeEmpty();
        result.RemovedRecipeIds.ShouldBeEmpty();
        list.RecipeIds.ShouldBe([IdOf(recipe)]);
        list.ManualItems.ShouldBe([item]);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            _handler.Handle(new ClearCheckedItemsInput(), cts.Token)
        );
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new ClearCheckedItemsHandler(Substitute.For<ILogger<ClearCheckedItemsHandler>>(), null!)
        );
    }
}
