using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.GetRecipe;

public sealed class GetRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetRecipeHandler _handler;
    private readonly ILogger<GetRecipeHandler> _logger = Substitute.For<ILogger<GetRecipeHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetRecipeHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetRecipeHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnRecipeWithComponentsResponse()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var recipe = Recipe.Create(
            new Id<Recipe>(Guid.NewGuid().ToString()),
            market.Id,
            _fixture.Create<string>(),
            _fixture.Create<decimal>(),
            [.. _fixture.CreateMany<RecipeComponent>(2)],
            [.. _fixture.CreateMany<RecipeComponent>(1)]
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(recipe);

        var query = new GetRecipeInput(recipe.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(recipe.Id);
        result.MarketId.ShouldBe(recipe.MarketId);
        result.Name.ShouldBe(recipe.Name);
        result.Cost.ShouldBe(recipe.Cost);
        result.Inputs.ShouldNotBeNull();
        result.Outputs.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetRecipeInput(new Id<Recipe>(_fixture.Create<string>()));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetRecipeInput(new Id<Recipe>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
