using Ardalis.GuardClauses;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using EventStoreOperations = Marten.Events.IEventStoreOperations;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.RevertRecipe;

public sealed class RevertRecipeHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly EventStoreOperations _events = Substitute.For<EventStoreOperations>();
    private readonly RevertRecipeHandler _handler;

    public RevertRecipeHandlerTests()
    {
        _session.Events.Returns(_events);
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);

        _handler = new RevertRecipeHandler(Substitute.For<ILogger<RevertRecipeHandler>>(), _store);
    }

    private static Recipe BuildCurrentState(Guid id)
    {
        var current = Recipe
            .Create(
                id,
                "Chocolate Cake",
                "https://example.com/cake",
                [new Step("Mix and bake.")],
                [new Ingredient(4m, "tablespoons", "granulated sugar")],
                "Best served warm."
            )
            .State;

        return Recipe.Apply(new RecipeTitleChanged("Updated Title"), current);
    }

    private static Recipe BuildHistoricalState(Guid id)
    {
        return Recipe
            .Create(
                id,
                "Chocolate Cake",
                "https://example.com/cake",
                [new Step("Mix and bake.")],
                [new Ingredient(4m, "tablespoons", "granulated sugar")],
                "Best served warm."
            )
            .State;
    }

    private static RevertRecipeInput ValidInput(Guid recipeId, long targetVersion)
    {
        return new RevertRecipeInput(new Id<Recipe>(recipeId.ToString()), targetVersion);
    }

    private void StubStreamState(Guid id, long version)
    {
        _events
            .FetchStreamStateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StreamState?>(new StreamState { Id = id, Version = version }));
    }

    [Fact]
    public async Task Handle_WhenRevertingToEarlierVersion_ShouldAppendRecipeRevertedEvent()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        StubStreamState(id, 2);
        var current = BuildCurrentState(id);
        var historical = BuildHistoricalState(id);

        _session
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Recipe?>(current));
        _events
            .AggregateStreamAsync<Recipe>(
                Arg.Any<Guid>(),
                Arg.Any<long>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<Recipe?>(),
                Arg.Any<long>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<Recipe?>(historical));

        object[]? capturedEvents = null;
        Guid? capturedStreamId = null;
        _events
            .When(e => e.Append(Arg.Any<Guid>(), Arg.Any<object[]>()))
            .Do(call =>
            {
                capturedStreamId = call.Arg<Guid>();
                capturedEvents = call.Arg<object[]>();
            });

        // Act
        var result = await _handler.Handle(ValidInput(id, 1), ct);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<IdResponse<Recipe>>();
        result.Id.ShouldBe(current.RecipeId);

        capturedStreamId.ShouldBe(id);
        capturedEvents.ShouldNotBeNull();
        var reverted = capturedEvents![0].ShouldBeOfType<RecipeReverted>();
        reverted.TargetVersion.ShouldBe(1);
        reverted.Title.ShouldBe("Chocolate Cake");
        reverted.SourceUrl.ShouldBe("https://example.com/cake");
        reverted.Steps.Count.ShouldBe(1);
        reverted.Ingredients.Count.ShouldBe(1);
        reverted.Notes.ShouldBe("Best served warm.");

        await _session.Received(1).SaveChangesAsync(ct);
    }

    [Fact]
    public async Task Handle_WhenTargetIsLatestVersion_ShouldNotAppendOrSave()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        StubStreamState(id, 2);

        // Act
        var result = await _handler.Handle(ValidInput(id, 2), ct);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(new Id<Recipe>(id.ToString()));
        _events.DidNotReceive().Append(Arg.Any<Guid>(), Arg.Any<object[]>());
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _session
            .DidNotReceive()
            .LoadAsync<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTargetVersionExceedsCurrent_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        StubStreamState(id, 2);

        var input = ValidInput(id, 3);

        // Act
        var revert = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await revert.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenTargetVersionIsBelowOne_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var id = Guid.NewGuid();
        StubStreamState(id, 2);

        var input = ValidInput(id, 0);

        // Act
        var revert = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await revert.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _events
            .FetchStreamStateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StreamState?>(null));

        var input = ValidInput(Guid.NewGuid(), 1);

        // Act
        var revert = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await revert.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRecipeIdIsNotAGuid_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new RevertRecipeInput(new Id<Recipe>("not-a-guid"), 1);

        // Act
        var revert = async () => await _handler.Handle(input, CancellationToken.None);

        // Assert
        await revert.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var input = ValidInput(Guid.NewGuid(), 1);
        var ct = new CancellationToken(true);

        // Act
        var revert = async () => await _handler.Handle(input, ct);

        // Assert
        await revert.ShouldThrowAsync<OperationCanceledException>();
    }
}
