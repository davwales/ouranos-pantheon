using Ardalis.GuardClauses;
using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.ReimportRecipe;

public sealed class ReimportRecipeHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IEventStoreOperations _events = Substitute.For<IEventStoreOperations>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly ReimportRecipeHandler _handler;

    public ReimportRecipeHandlerTests()
    {
        _session.Events.Returns(_events);
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);

        _handler = new ReimportRecipeHandler(
            Substitute.For<ILogger<ReimportRecipeHandler>>(),
            _store,
            _bus
        );
    }

    private static Recipe BuildRecipe(Guid id, string? sourceUrl = "https://example.com/cake")
    {
        return Recipe
            .Create(
                id,
                "Chocolate Cake",
                sourceUrl,
                [new Step("Mix and bake.")],
                [new Ingredient(4m, "tablespoons", "granulated sugar")],
                "Best served warm."
            )
            .State;
    }

    private static ReimportRecipeInput ValidInput(Guid recipeId)
    {
        return new ReimportRecipeInput(new Id<Recipe>(recipeId.ToString()));
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldAppendRecipeImportStartedEventAndRepublishRequest()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        var recipe = BuildRecipe(id);
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));

        ImportRecipeRequested? published = null;
        Guid? capturedStreamId = null;
        object[]? capturedEvents = null;
        _bus.When(b => b.PublishAsync(Arg.Any<ImportRecipeRequested>()))
            .Do(call => published = call.Arg<ImportRecipeRequested>());
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call =>
            {
                capturedStreamId = call.Arg<Guid>();
                capturedEvents = call.Arg<object[]>();
            });

        // Act
        var result = await _handler.Handle(ValidInput(id), ct);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(recipe.RecipeId);

        capturedStreamId.ShouldBe(id);
        capturedEvents.ShouldNotBeNull();
        capturedEvents!.Length.ShouldBe(1);
        var @event = capturedEvents[0].ShouldBeOfType<RecipeImportStarted>();
        @event.StartedAt.ShouldNotBe(default);

        published.ShouldNotBeNull();
        published!.RecipeId.ShouldBe(recipe.RecipeId);
        published.Url.ShouldBe(recipe.SourceUrl);
        published.RequestedAt.ShouldNotBe(default);

        await _session.Received(1).SaveChangesAsync(ct);
        await _bus.Received(1).PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(null));

        var input = ValidInput(Guid.NewGuid());

        // Act
        var reimport = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await reimport.ShouldThrowAsync<NotFoundException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenRecipeIdIsNotAGuid_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new ReimportRecipeInput(new Id<Recipe>("not-a-guid"));

        // Act
        var reimport = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await reimport.ShouldThrowAsync<NotFoundException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenRecipeHasNoSourceUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var recipe = BuildRecipe(id, sourceUrl: null);
        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(recipe));

        // Act
        var reimport = async () => await _handler.Handle(ValidInput(id), CancellationToken.None);

        // Assert
        await reimport.ShouldThrowAsync<InvalidOperationException>();
        _events.DidNotReceive().Append(Arg.Any<Guid>(), Arg.Any<object[]>());
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var input = ValidInput(Guid.NewGuid());
        var ct = new CancellationToken(true);

        // Act
        var reimport = async () => await _handler.Handle(input, ct);

        // Assert
        await reimport.ShouldThrowAsync<OperationCanceledException>();
        await _bus.DidNotReceive().PublishAsync(Arg.Any<ImportRecipeRequested>());
    }
}
