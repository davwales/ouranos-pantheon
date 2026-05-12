using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Recipes;

public sealed class RecipeTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = _fixture.Create<Id<Recipe>>();
        var market = _fixture.Create<Market>();
        var name = _fixture.Create<string>();
        var cost = _fixture.Create<decimal>();
        var inputs = _fixture.CreateMany<RecipeComponent>().ToList();
        var outputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var recipe = Recipe.Create(id, market.Id, name, cost, inputs, outputs);

        // Assert
        recipe.Id.ShouldBe(id);
        recipe.MarketId.ShouldBe(market.Id);
        recipe.Name.ShouldBe(name);
        recipe.Cost.ShouldBe(cost);
        recipe.Inputs.ShouldBe(inputs);
        recipe.Outputs.ShouldBe(outputs);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = _fixture.Create<Id<Recipe>>();
        var market = _fixture.Create<Market>();
        var cost = _fixture.Create<decimal>();
        var inputs = _fixture.CreateMany<RecipeComponent>().ToList();
        var outputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var create = () => Recipe.Create(id, market.Id, name!, cost, inputs, outputs);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullInputs_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Recipe>>();
        var market = _fixture.Create<Market>();
        var name = _fixture.Create<string>();
        var cost = _fixture.Create<decimal>();
        var outputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var create = () => Recipe.Create(id, market.Id, name, cost, null!, outputs);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullOutputs_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Recipe>>();
        var market = _fixture.Create<Market>();
        var name = _fixture.Create<string>();
        var cost = _fixture.Create<decimal>();
        var inputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var create = () => Recipe.Create(id, market.Id, name, cost, inputs, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Market_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var id = _fixture.Create<Id<Recipe>>();
        var market = _fixture.Create<Market>();
        var recipe = Recipe.Create(
            id,
            market.Id,
            "Test Recipe",
            100m,
            _fixture.CreateMany<RecipeComponent>().ToList(),
            _fixture.CreateMany<RecipeComponent>().ToList()
        );

        // Act
        var access = () => _ = recipe.Market;

        // Assert
        access.ShouldThrow<Exception>();
    }

    [Fact]
    public void Create_WhenMarketProvided_ShouldExposeMarket()
    {
        // Arrange
        var id = _fixture.Create<Id<Recipe>>();
        var market = _fixture.Create<Market>();

        // Act
        var recipe = Recipe.Create(
            id,
            market.Id,
            "Test Recipe",
            100m,
            _fixture.CreateMany<RecipeComponent>().ToList(),
            _fixture.CreateMany<RecipeComponent>().ToList(),
            market
        );

        // Assert
        recipe.Market.ShouldBe(market);
    }

    [Fact]
    public void Create_WhenMarketIdMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        var differentMarketId = _fixture.Create<Id<Market>>();

        // Act
        var create = () =>
            Recipe.Create(
                _fixture.Create<Id<Recipe>>(),
                differentMarketId,
                "Test Recipe",
                100m,
                _fixture.CreateMany<RecipeComponent>().ToList(),
                _fixture.CreateMany<RecipeComponent>().ToList(),
                market
            );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}
