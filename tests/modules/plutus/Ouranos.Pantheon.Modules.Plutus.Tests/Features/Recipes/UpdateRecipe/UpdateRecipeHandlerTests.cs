using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateRecipeHandler _handler;
    private readonly ILogger<UpdateRecipeHandler> _logger = Substitute.For<
        ILogger<UpdateRecipeHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public UpdateRecipeHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new UpdateRecipeHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateRecipeAndReturnId()
    {
        // Arrange
        var market = _fixture.Create<Market>();

        var existingRecipe = Recipe.Create(
            _fixture.Create<Id<Recipe>>(),
            market.Id,
            _fixture.Create<string>(),
            _fixture.Create<decimal>(),
            _fixture.CreateMany<RecipeComponent>().ToList(),
            _fixture.CreateMany<RecipeComponent>().ToList()
        );

        await _dbContext.SeedData(existingRecipe);
        var originalCreatedAt = existingRecipe.CreatedAt;
        var originalUpdatedAt = existingRecipe.UpdatedAt;
        SpinWait.SpinUntil(() => DateTimeOffset.UtcNow > originalUpdatedAt);

        var command = new UpdateRecipeInput(
            market.Id,
            existingRecipe.Id,
            _fixture.Create<string>(),
            _fixture.Create<decimal>(),
            _fixture.CreateMany<RecipeComponent>().ToList(),
            _fixture.CreateMany<RecipeComponent>().ToList()
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(command.RecipeId);

        var updatedRecipe = await _dbContext.Recipes.FindAsync(command.RecipeId);
        updatedRecipe.ShouldNotBeNull();
        updatedRecipe.Name.ShouldBe(command.Name);
        updatedRecipe.Cost.ShouldBe(command.Cost);
        updatedRecipe.Inputs.ShouldBe(command.Inputs, ignoreOrder: true);
        updatedRecipe.Outputs.ShouldBe(command.Outputs, ignoreOrder: true);
        updatedRecipe.CreatedAt.ShouldBe(originalCreatedAt);
        updatedRecipe.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    [Fact]
    public async Task Handle_WhenRecipeBelongsToAnotherMarket_ShouldThrowNotFoundException()
    {
        // Arrange
        var existingRecipe = Recipe.Create(
            _fixture.Create<Id<Recipe>>(),
            _fixture.Create<Id<Market>>(),
            _fixture.Create<string>(),
            _fixture.Create<decimal>(),
            _fixture.CreateMany<RecipeComponent>().ToList(),
            _fixture.CreateMany<RecipeComponent>().ToList()
        );

        await _dbContext.SeedData(existingRecipe);

        var command = new UpdateRecipeInput(
            _fixture.Create<Id<Market>>(),
            existingRecipe.Id,
            _fixture.Create<string>(),
            _fixture.Create<decimal>(),
            [],
            []
        );

        // Act
        var update = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await update.ShouldThrowAsync<NotFoundException>();
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
