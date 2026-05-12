using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateRecipeHandler _handler;
    private readonly ILogger<CreateRecipeHandler> _logger = Substitute.For<
        ILogger<CreateRecipeHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public CreateRecipeHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new CreateRecipeHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateRecipeAndReturnId()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        await _dbContext.SeedData(market);

        var command = new CreateRecipeInput(
            market.Id,
            _fixture.Create<string>(),
            _fixture.Create<decimal>(),
            _fixture.CreateMany<RecipeComponent>().ToList(),
            _fixture.CreateMany<RecipeComponent>().ToList()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldNotBe(default);

        var recipe = await _dbContext.Recipes.FindAsync(result.Id);
        recipe.ShouldNotBeNull();
        recipe.MarketId.ShouldBe(command.MarketId);
        recipe.Name.ShouldBe(command.Name);
        recipe.Cost.ShouldBe(command.Cost);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = _fixture.Create<CreateRecipeInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }
}
