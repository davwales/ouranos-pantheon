using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.GetRecipe;

public sealed class GetRecipeHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IQuerySession _session = Substitute.For<IQuerySession>();
    private readonly GetRecipeHandler _handler;

    public GetRecipeHandlerTests()
    {
        _store.QuerySession().Returns(_session);
        _handler = new GetRecipeHandler(Substitute.For<ILogger<GetRecipeHandler>>(), _store);
    }

    private static Recipe BuildRecipe(
        Guid id,
        string title = "Chocolate Cake",
        string? sourceUrl = "https://example.com/cake",
        List<Step>? steps = null,
        string notes = "Best served warm."
    )
    {
        var ingredients = new List<Ingredient>
        {
            new(4m, "tablespoons", "granulated sugar"),
            new(1m, "tablespoon", "ground cinnamon"),
        };

        return Recipe
            .Create(id, title, sourceUrl, steps ?? [new Step("Mix and bake.")], ingredients, notes)
            .State;
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnFullRecipeResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var recipe = BuildRecipe(id);
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));

        var query = new GetRecipeInput(new Id<Recipe>(id.ToString()));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(recipe.RecipeId);
        result.Title.ShouldBe(recipe.Title);
        result.SourceUrl.ShouldBe(recipe.SourceUrl);
        result.Steps.Count.ShouldBe(1);
        result.Steps[0].Text.ShouldBe("Mix and bake.");
        result.Notes.ShouldBe(recipe.Notes);
        result.CreatedAt.ShouldBe(recipe.CreatedAt);
        result.Ingredients.Count.ShouldBe(2);
        result.Ingredients[0].Quantity.ShouldBe(4m);
        result.Ingredients[0].Unit.ShouldBe("tablespoons");
        result.Ingredients[0].Name.ShouldBe("granulated sugar");
        result.Ingredients[1].Quantity.ShouldBe(1m);
        result.Ingredients[1].Unit.ShouldBe("tablespoon");
        result.Ingredients[1].Name.ShouldBe("ground cinnamon");
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(null));

        var query = new GetRecipeInput(new Id<Recipe>(Guid.NewGuid().ToString()));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenRecipeIdIsNotAGuid_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetRecipeInput(new Id<Recipe>("not-a-guid"));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetRecipeInput(new Id<Recipe>(Guid.NewGuid().ToString()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
