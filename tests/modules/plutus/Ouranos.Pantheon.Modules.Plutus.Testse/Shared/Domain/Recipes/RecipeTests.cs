using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

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
        var recipe = Recipe.Create(id, market, name, cost, inputs, outputs);

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
        var create = () => Recipe.Create(id, market, name!, cost, inputs, outputs);

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
        var create = () => Recipe.Create(id, market, name, cost, null!, outputs);

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
        var create = () => Recipe.Create(id, market, name, cost, inputs, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}
