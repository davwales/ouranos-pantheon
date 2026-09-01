using Ardalis.GuardClauses;
using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.ShoppingLists.ToggleRecipe;

public sealed class ToggleRecipeHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly ToggleRecipeHandler _handler;

    public ToggleRecipeHandlerTests()
    {
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);
        _handler = new ToggleRecipeHandler(Substitute.For<ILogger<ToggleRecipeHandler>>(), _store);
    }

    private static Recipe BuildRecipe(Guid id)
    {
        return Recipe
            .Create(
                id,
                "Chocolate Cake",
                null,
                [new Step("Mix and bake.")],
                [new Ingredient(4m, "tablespoons", "granulated sugar")],
                "Best served warm."
            )
            .State;
    }

    private static ToggleRecipeInput Input(Guid id)
    {
        return new ToggleRecipeInput(new Id<Recipe>(id.ToString()));
    }

    [Fact]
    public async Task Handle_WhenRecipeNotInList_ShouldAddAndReturnTrue()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipe = BuildRecipe(recipeId);
        var list = new ShoppingList();

        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var result = await _handler.Handle(Input(recipeId), CancellationToken.None);

        // Assert
        result.IsInList.ShouldBeTrue();
        list.RecipeIds.ShouldContain(new Id<Recipe>(recipeId.ToString()));
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRecipeAlreadyInList_ShouldRemoveAndReturnFalse()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipe = BuildRecipe(recipeId);
        var recipeIdValue = new Id<Recipe>(recipeId.ToString());
        var list = new ShoppingList { RecipeIds = [recipeIdValue] };

        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(list));

        // Act
        var result = await _handler.Handle(Input(recipeId), CancellationToken.None);

        // Assert
        result.IsInList.ShouldBeFalse();
        result.RecipeId.ShouldBe(recipeIdValue);
        list.RecipeIds.ShouldNotContain(recipeIdValue);
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenListDoesNotExist_ShouldCreateAndAddRecipe()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var recipe = BuildRecipe(recipeId);

        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));
        _session
            .LoadAsync<ShoppingList>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingList?>(null));

        // Act
        var result = await _handler.Handle(Input(recipeId), CancellationToken.None);

        // Assert
        result.IsInList.ShouldBeTrue();
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(null));

        // Act
        var toggle = async () =>
            await _handler.Handle(Input(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await toggle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRecipeIdIsNotAGuid_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new ToggleRecipeInput(new Id<Recipe>("not-a-guid"));

        // Act
        var toggle = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await toggle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var ct = new CancellationToken(true);

        // Act
        var toggle = async () => await _handler.Handle(Input(Guid.NewGuid()), ct);

        // Assert
        await toggle.ShouldThrowAsync<OperationCanceledException>();
    }
}
