using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.CreateRecipe;

public sealed class CreateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateRecipeHandler _handler;
    private readonly ILogger<CreateRecipeHandler> _logger = Substitute.For<ILogger<CreateRecipeHandler>>();
    private readonly PlutusDbContext _dbContext = Substitute.For<PlutusDbContext>(
        Substitute.For<DbContextOptions<PlutusDbContext>>()
    );

    public CreateRecipeHandlerTests()
    {
        _handler = new CreateRecipeHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateRecipeAndReturnId()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        var expectedId = _fixture.Create<Id<Recipe>>();
        var command = _fixture.Build<CreateRecipeInput>()
            .With(x => x.MarketId, market.Id)
            .Create();

        var marketsDbSet = Substitute.For<DbSet<Market>>();
        var recipesDbSet = Substitute.For<DbSet<Recipe>>();
        
        _dbContext.Markets.Returns(marketsDbSet);
        _dbContext.Recipes.Returns(recipesDbSet);

        marketsDbSet.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Market, bool>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(market);

        var entryMock = Substitute.For<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Recipe>>();
        recipesDbSet.AddAsync(Arg.Any<Recipe>(), Arg.Any<CancellationToken>())
            .Returns(_ => new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Recipe>>(entryMock));

        _dbContext.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldNotBe(default);

        await marketsDbSet.Received(1).FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Market, bool>>>(),
            Arg.Any<CancellationToken>()
        );

        await recipesDbSet.Received(1).AddAsync(
            Arg.Is<Recipe>(r =>
                r.MarketId == command.MarketId &&
                r.Name == command.Name &&
                r.Cost == command.Cost
            ),
            Arg.Any<CancellationToken>()
        );

        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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
