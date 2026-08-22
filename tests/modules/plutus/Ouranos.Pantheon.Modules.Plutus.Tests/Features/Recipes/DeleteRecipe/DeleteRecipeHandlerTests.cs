using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.DeleteRecipe;

public sealed class DeleteRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteRecipeHandler _handler;
    private readonly ILogger<DeleteRecipeHandler> _logger = Substitute.For<
        ILogger<DeleteRecipeHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public DeleteRecipeHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new DeleteRecipeHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteRecipeAndReturnId()
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
            _fixture.CreateMany<RecipeComponent>().ToList(),
            _fixture.CreateMany<RecipeComponent>().ToList()
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(recipe);

        var command = new DeleteRecipeInput(recipe.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(recipe.Id);

        var deletedRecipe = await _dbContext.Recipes.FindAsync(recipe.Id);
        deletedRecipe.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteRecipeInput(new Id<Recipe>(_fixture.Create<string>()));

        // Act
        var delete = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await delete.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteRecipeInput(new Id<Recipe>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var delete = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await delete.ShouldThrowAsync<OperationCanceledException>();
    }
}
