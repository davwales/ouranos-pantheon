using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeConsumerTests
{
    private static ImportRecipeRequested ValidMessage()
    {
        return new ImportRecipeRequested(
            new Id<Recipe>(Guid.NewGuid().ToString()),
            "https://example.com/recipe",
            DateTimeOffset.UtcNow
        );
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldComplete()
    {
        // Arrange
        var consumer = new ImportRecipeConsumer(Substitute.For<ILogger<ImportRecipeConsumer>>());
        var message = ValidMessage();

        // Act
        var act = async () => await consumer.Handle(message, CancellationToken.None);

        // Assert
        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var consumer = new ImportRecipeConsumer(Substitute.For<ILogger<ImportRecipeConsumer>>());
        var message = ValidMessage();
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await consumer.Handle(message, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
