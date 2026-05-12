using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.GetRecipeTrades;

public sealed class GetRecipeTradesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetRecipeTradesHandler _handler;
    private readonly ILogger<GetRecipeTradesHandler> _logger = Substitute.For<
        ILogger<GetRecipeTradesHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetRecipeTradesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetRecipeTradesHandler(
            _logger,
            _dbContext,
            Options.Create(new QueryOptions())
        );
    }

    [Fact]
    public async Task Handle_WhenNoRecipes_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetRecipeTradesInput(new Id<Market>(_fixture.Create<string>()), Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenRecipesExistWithNoPriceData_ShouldReturnZeroMargins()
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
            10m,
            [new RecipeComponent(new Id<Symbol>(Guid.NewGuid().ToString()), "Input", 1)],
            [new RecipeComponent(new Id<Symbol>(Guid.NewGuid().ToString()), "Output", 1)]
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(recipe);

        var query = new GetRecipeTradesInput(market.Id, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        result.Items.ElementAt(0).LatestBuyPrice.ShouldBe(0m);
        result.Items.ElementAt(0).LatestSellPrice.ShouldBe(0m);
        result.Items.ElementAt(0).LatestMargin.ShouldBe(0m);
    }

    [Fact]
    public async Task Handle_WhenRecipesExistWithPriceData_ShouldCalculateMargins()
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
            [new RecipeComponent(inputSymbolId, "Input", 1)],
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
            price: 200m,
            volume: 1m,
            DateTimeOffset.UtcNow
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(recipe);
        await _dbContext.SeedData(inputTrade, outputTrade);

        var query = new GetRecipeTradesInput(market.Id, TimeFrame.AllTime, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        var item = result.Items.ElementAt(0);
        item.LatestBuyPrice.ShouldBe(105m);
        item.LatestSellPrice.ShouldBe(200m);
        item.LatestMargin.ShouldBe(95m);
        item.RecipeId.ShouldBe(recipe.Id);
        item.RecipeName.ShouldBe(recipe.Name);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetRecipeTradesInput(new Id<Market>(_fixture.Create<string>()), Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
