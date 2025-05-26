using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

namespace Ouranos.Pantheon.Service.Plutus.Application.Tests.Commands.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateRecipeHandler _handler;
    private readonly ILogger<UpdateRecipeHandler> _logger = Substitute.For<ILogger<UpdateRecipeHandler>>();
    private readonly IRepository<Recipe> _recipeRepository = Substitute.For<IRepository<Recipe>>();

    public UpdateRecipeHandlerTests()
    {
        _handler = new UpdateRecipeHandler(_logger, _recipeRepository);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateRecipeAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<UpdateRecipeInput>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(command.RecipeId);

        await _recipeRepository.Received(1).Update(
            Arg.Is<Recipe>(r =>
                r.Id == command.RecipeId &&
                r.MarketId == command.MarketId &&
                r.Name == command.Name &&
                r.Cost == command.Cost &&
                r.Inputs == command.Inputs &&
                r.Outputs == command.Outputs
            ),
            Arg.Any<CancellationToken>()
        );
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
