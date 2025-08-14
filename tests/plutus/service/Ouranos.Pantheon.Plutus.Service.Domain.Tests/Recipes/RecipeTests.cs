using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Tests.Recipes;

public sealed class RecipeTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Recipe>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var name = _fixture.Create<string>();
        var cost = _fixture.Create<decimal>();
        var inputs = _fixture.CreateMany<RecipeComponent>().ToList();
        var outputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var recipe = new Recipe(id, marketId, name, cost, inputs, outputs);

        // Assert
        recipe.Id.ShouldBe(id);
        recipe.MarketId.ShouldBe(marketId);
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
        var id = new Id<Recipe>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var cost = _fixture.Create<decimal>();
        var inputs = _fixture.CreateMany<RecipeComponent>().ToList();
        var outputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var create = () => new Recipe(id, marketId, name!, cost, inputs, outputs);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullInputs_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Recipe>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var name = _fixture.Create<string>();
        var cost = _fixture.Create<decimal>();
        var outputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var create = () => new Recipe(id, marketId, name, cost, null!, outputs);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullOutputs_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Recipe>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var name = _fixture.Create<string>();
        var cost = _fixture.Create<decimal>();
        var inputs = _fixture.CreateMany<RecipeComponent>().ToList();

        // Act
        var create = () => new Recipe(id, marketId, name, cost, inputs, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}