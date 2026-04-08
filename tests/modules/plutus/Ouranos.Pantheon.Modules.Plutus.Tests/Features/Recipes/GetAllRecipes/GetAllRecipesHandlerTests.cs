using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.GetAllRecipes;

public sealed class GetAllRecipesHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllRecipesHandler _handler;
    private readonly ILogger<GetAllRecipesHandler> _logger = Substitute.For<ILogger<GetAllRecipesHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetAllRecipesHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllRecipesHandler(_logger, _dbContext, Options.Create(new QueryOptions()));
    }

    [Fact]
    public async Task Handle_WhenRecipesExist_ShouldReturnPagedRecipes()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var recipes = Enumerable.Range(0, 3).Select(_ => Recipe.Create(
                new Id<Recipe>(Guid.NewGuid().ToString()),
                market.Id,
                _fixture.Create<string>(),
                _fixture.Create<decimal>(),
                _fixture.CreateMany<RecipeComponent>().ToList(),
                _fixture.CreateMany<RecipeComponent>().ToList()
            )
        ).ToArray();

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(recipes);

        var query = new GetAllRecipesInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        var item = result.Items.First();
        item.Id.ShouldNotBe(default);
        item.MarketId.ShouldNotBe(default);
        item.Name.ShouldNotBeNull();
        item.Cost.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task Handle_WhenNoRecipes_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetAllRecipesInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllRecipesInput(Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
