using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateRecipeHandler _handler;
    private readonly ILogger<UpdateRecipeHandler> _logger = Substitute.For<ILogger<UpdateRecipeHandler>>();
    private readonly PlutusDbContext _dbContext = Substitute.For<PlutusDbContext>(
        Substitute.For<DbContextOptions<PlutusDbContext>>()
    );

    public UpdateRecipeHandlerTests()
    {
        _handler = new UpdateRecipeHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateRecipeAndReturnId()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        var existingRecipe = _fixture.Create<Recipe>();
        var command = _fixture.Build<UpdateRecipeInput>()
            .With(x => x.MarketId, market.Id)
            .With(x => x.RecipeId, existingRecipe.Id)
            .Create();

        var marketsDbSet = Substitute.For<DbSet<Market>>();
        var recipesDbSet = Substitute.For<DbSet<Recipe>>();
        
        _dbContext.Markets.Returns(marketsDbSet);
        _dbContext.Recipes.Returns(recipesDbSet);

        marketsDbSet.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Market, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(market);

        recipesDbSet.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Recipe, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(existingRecipe);

        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(command.RecipeId);

        marketsDbSet.Received(1).FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Market, bool>>>(),
            Arg.Any<CancellationToken>()
        );

        recipesDbSet.Received(1).Remove(Arg.Is<Recipe>(r => r.Id == existingRecipe.Id));

        recipesDbSet.Received(1).AddAsync(
            Arg.Any<Recipe>(),
            Arg.Any<CancellationToken>()
        );

        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = _fixture.Create<UpdateRecipeInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var update = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await update.ShouldThrowAsync<OperationCanceledException>();
    }
}
