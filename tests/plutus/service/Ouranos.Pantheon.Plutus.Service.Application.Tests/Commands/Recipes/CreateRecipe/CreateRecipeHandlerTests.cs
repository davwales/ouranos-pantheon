using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Commands.Recipes.CreateRecipe;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Commands.Recipes.CreateRecipe;

public sealed class CreateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateRecipeHandler _handler;
    private readonly ILogger<CreateRecipeHandler> _logger = Substitute.For<ILogger<CreateRecipeHandler>>();
    private readonly IPlutusUnitOfWork _unitOfWork = Substitute.For<IPlutusUnitOfWork>();

    public CreateRecipeHandlerTests()
    {
        _handler = new CreateRecipeHandler(_logger, _unitOfWork);
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

        _unitOfWork.Recipes.CreateId().Returns(expectedId);
        _unitOfWork.Markets.Read(command.MarketId, CancellationToken.None).Returns(market);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(expectedId);

        await _unitOfWork.Recipes.Received(1).Create(
            Arg.Is<Recipe>(r =>
                r.Id == expectedId &&
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
        var command = _fixture.Create<CreateRecipeInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var create = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await create.ShouldThrowAsync<OperationCanceledException>();
    }
}