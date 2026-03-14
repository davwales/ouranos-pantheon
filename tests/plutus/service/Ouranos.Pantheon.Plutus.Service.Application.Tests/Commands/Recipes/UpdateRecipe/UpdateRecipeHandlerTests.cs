using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Commands.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Commands.Recipes.UpdateRecipe;

public sealed class UpdateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly UpdateRecipeHandler _handler;
    private readonly ILogger<UpdateRecipeHandler> _logger = Substitute.For<ILogger<UpdateRecipeHandler>>();
    private readonly IPlutusUnitOfWork _unitOfWork = Substitute.For<IPlutusUnitOfWork>();

    public UpdateRecipeHandlerTests()
    {
        _handler = new UpdateRecipeHandler(_logger, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldUpdateRecipeAndReturnId()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        var command = _fixture.Build<UpdateRecipeInput>()
            .With(x => x.MarketId, market.Id)
            .Create();

        _unitOfWork.Markets.Read(market.Id, CancellationToken.None).Returns(market);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(command.RecipeId);

        await _unitOfWork.Recipes.Received(1).Update(
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