using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeHandlerTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly ImportRecipeHandler _handler;

    public ImportRecipeHandlerTests()
    {
        _handler = new ImportRecipeHandler(Substitute.For<ILogger<ImportRecipeHandler>>(), _bus);
    }

    private static ImportRecipeInput ValidInput(string url = "https://example.com/recipe")
    {
        return new ImportRecipeInput(url);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldPublishRequestAndReturnId()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = ValidInput();
        ImportRecipeRequested? published = null;

        _bus.When(b => b.PublishAsync(Arg.Any<ImportRecipeRequested>()))
            .Do(call => published = call.Arg<ImportRecipeRequested>());

        // Act
        var result = await _handler.Handle(input, ct);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBeOfType<Id<Recipe>>();
        result.Id.Value.ShouldNotBeNullOrEmpty();

        published.ShouldNotBeNull();
        published!.Url.ShouldBe(input.Url);
        published.RecipeId.ShouldBe(result.Id);
        published.RequestedAt.ShouldNotBe(default);

        await _bus.Received(1).PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var input = ValidInput();
        var ct = new CancellationToken(true);

        // Act
        var act = async () => await _handler.Handle(input, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenUrlIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var input = ValidInput(null!);

        // Act
        var act = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenUrlIsWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var input = ValidInput("   ");

        // Act
        var act = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenUrlIsRelative_ShouldThrowArgumentException()
    {
        // Arrange
        var input = ValidInput("/recipes/cake");

        // Act
        var act = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenUrlUsesUnsupportedScheme_ShouldThrowArgumentException()
    {
        // Arrange
        var input = ValidInput("ftp://example.com/recipe");

        // Act
        var act = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenUrlExceedsMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var input = ValidInput($"https://example.com/{new string('a', 2_000)}");

        // Act
        var act = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<ArgumentOutOfRangeException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }
}
