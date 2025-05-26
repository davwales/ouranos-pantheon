using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Recipes.CreateRecipe;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

namespace Ouranos.Pantheon.Service.Plutus.Application.Tests.Commands.Recipes.CreateRecipe;

public sealed class CreateRecipeHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly CreateRecipeHandler _handler;
    private readonly ILogger<CreateRecipeHandler> _logger = Substitute.For<ILogger<CreateRecipeHandler>>();
    private readonly IRepository<Recipe> _recipeRepository = Substitute.For<IRepository<Recipe>>();

    public CreateRecipeHandlerTests()
    {
        _handler = new CreateRecipeHandler(_logger, _recipeRepository);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateRecipeAndReturnId()
    {
        // Arrange
        var command = _fixture.Create<CreateRecipeInput>();
        var expectedId = new Id<Recipe>(_fixture.Create<string>());

        _recipeRepository.CreateId().Returns(expectedId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(expectedId);

        await _recipeRepository.Received(1).Create(
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
