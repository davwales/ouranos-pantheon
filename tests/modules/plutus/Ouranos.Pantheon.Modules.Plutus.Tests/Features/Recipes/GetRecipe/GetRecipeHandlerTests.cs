using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.GetRecipe;

public sealed class GetRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetRecipeHandler _handler;
    private readonly ILogger<GetRecipeHandler> _logger = Substitute.For<
        ILogger<GetRecipeHandler>
    >();
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
    public async Task Handle_WhenTradesExist_ShouldReturnPricedComponents()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var inputSymbolId = new Id<Symbol>(Guid.NewGuid().ToString());
        var outputSymbolId = new Id<Symbol>(Guid.NewGuid().ToString());

        var recipe = Recipe.Create(
            new Id<Recipe>(Guid.NewGuid().ToString()),
            market.Id,
            _fixture.Create<string>(),
            cost: 5m,
            [new RecipeComponent(inputSymbolId, "Input", 2)],
            [new RecipeComponent(outputSymbolId, "Output", 1)]
        );

        var inputTrade = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            inputSymbolId,
            price: 100m,
            volume: 1m,
            DateTimeOffset.UtcNow
        );
        var outputTrade = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            outputSymbolId,
            price: 300m,
            volume: 1m,
            DateTimeOffset.UtcNow
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(recipe);
        await _dbContext.SeedData(inputTrade, outputTrade);

        var query = new GetRecipeInput(recipe.Id, TimeFrame.AllTime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Inputs.Count.ShouldBe(1);
        result.Outputs.Count.ShouldBe(1);

        var input = result.Inputs[0];
        input.LatestPrice.ShouldBe(100m);
        input.AveragePrice.ShouldBe(100m);
        input.TotalValue.ShouldBe(200m); // 100 * quantity 2
        input.Volume.ShouldBe(1m);

        var output = result.Outputs[0];
        output.LatestPrice.ShouldBe(300m);
        output.AveragePrice.ShouldBe(300m);
        output.TotalValue.ShouldBe(300m); // 300 * quantity 1
        output.Volume.ShouldBe(1m);
    }

    [Fact]
    public async Task Handle_WhenNoTradesExist_ShouldReturnNullPriceFields()
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
            cost: 5m,
            [new RecipeComponent(new Id<Symbol>(Guid.NewGuid().ToString()), "Input", 1)],
            [new RecipeComponent(new Id<Symbol>(Guid.NewGuid().ToString()), "Output", 1)]
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(recipe);

        var query = new GetRecipeInput(recipe.Id, TimeFrame.AllTime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Inputs[0].LatestPrice.ShouldBeNull();
        result.Inputs[0].AveragePrice.ShouldBeNull();
        result.Inputs[0].TotalValue.ShouldBeNull();
        result.Inputs[0].Volume.ShouldBeNull();
        result.Outputs[0].LatestPrice.ShouldBeNull();
        result.Outputs[0].AveragePrice.ShouldBeNull();
        result.Outputs[0].TotalValue.ShouldBeNull();
        result.Outputs[0].Volume.ShouldBeNull();
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
