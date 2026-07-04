using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.GetAllRecipes;

public sealed class GetAllRecipesHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly GetAllRecipesHandler _handler;

    public GetAllRecipesHandlerTests()
    {
        _handler = new GetAllRecipesHandler(
            Substitute.For<ILogger<GetAllRecipesHandler>>(),
            _store,
            Options.Create(new QueryOptions())
        );
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

    [Fact]
    public async Task Handle_WhenSkipExceedsMax_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var query = new GetAllRecipesInput(Skip: 10001, Take: 10);

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenTakeExceedsMax_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var query = new GetAllRecipesInput(Skip: 0, Take: 101);

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenTakeBelowMin_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var query = new GetAllRecipesInput(Skip: 0, Take: 0);

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }
}
