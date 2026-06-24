using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application.Common;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.GetAllRecipes;

public sealed class GetAllRecipesHandlerTests
{
    private readonly GetAllRecipesHandler _handler = new(
        Substitute.For<ILogger<GetAllRecipesHandler>>(),
        Substitute.For<IHestiaMartenStore>(),
        Options.Create(new QueryOptions())
    );

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
}
