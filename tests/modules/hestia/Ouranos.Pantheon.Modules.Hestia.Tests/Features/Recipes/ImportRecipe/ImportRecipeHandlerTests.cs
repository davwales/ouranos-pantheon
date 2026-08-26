using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ImportRecipe;

public sealed class ImportRecipeHandlerTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IEventStoreOperations _events = Substitute.For<IEventStoreOperations>();
    private readonly ImportRecipeHandler _handler;

    public ImportRecipeHandlerTests()
    {
        _session.Events.Returns(_events);
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);

        _handler = new ImportRecipeHandler(
            Substitute.For<ILogger<ImportRecipeHandler>>(),
            _bus,
            _store
        );
    }

    private static ImportRecipeInput ValidInput(string url = "https://example.com/recipe")
    {
        return new ImportRecipeInput(url);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldCreateRecipeStreamAndPublishRequest()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = ValidInput();
        ImportRecipeRequested? published = null;
        Guid? capturedStreamId = null;
        object[]? capturedEvents = null;

        _bus.When(b => b.PublishAsync(Arg.Any<ImportRecipeRequested>()))
            .Do(call => published = call.Arg<ImportRecipeRequested>());
        _events
            .When(e => e.StartStream(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call =>
            {
                capturedStreamId = call.Arg<Guid>();
                capturedEvents = call.Arg<object[]>();
            });

        // Act
        var result = await _handler.Handle(input, ct);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBeOfType<Id<Recipe>>();
        result.Id.Value.ShouldNotBeNullOrEmpty();

        capturedStreamId.ShouldNotBeNull();
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(2);
        var created = capturedEvents[0].ShouldBeOfType<RecipeCreated>();
        created.Id.ShouldBe(capturedStreamId.Value);
        created.Title.ShouldBe("New Recipe");
        created.SourceUrl.ShouldBe(input.Url);
        created.Steps.ShouldBeEmpty();
        created.Ingredients.ShouldBeEmpty();
        var started = capturedEvents[1].ShouldBeOfType<RecipeImportStarted>();
        started.StartedAt.ShouldNotBe(default);

        published.ShouldNotBeNull();
        published!.Url.ShouldBe(input.Url);
        published.RecipeId.ShouldBe(result.Id);
        published.RecipeId.Value.ShouldBe(capturedStreamId.Value.ToString());
        published.RequestedAt.ShouldNotBe(default);

        await _session.Received(1).SaveChangesAsync(ct);
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
        _store.DidNotReceive().LightweightSession();
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
        _store.DidNotReceive().LightweightSession();
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
        _store.DidNotReceive().LightweightSession();
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
        _store.DidNotReceive().LightweightSession();
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
        _store.DidNotReceive().LightweightSession();
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
        _store.DidNotReceive().LightweightSession();
    }
}
