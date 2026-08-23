using Ardalis.GuardClauses;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using QueryEventStore = Marten.Events.IQueryEventStore;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.GetRecipeHistory;

public sealed class GetRecipeHistoryHandlerTests
{
    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IQuerySession _session = Substitute.For<IQuerySession>();
    private readonly QueryEventStore _events = Substitute.For<QueryEventStore>();
    private readonly GetRecipeHistoryHandler _handler;

    public GetRecipeHistoryHandlerTests()
    {
        _session.Events.Returns(_events);
        _store.QuerySession().Returns(_session);

        _handler = new GetRecipeHistoryHandler(
            Substitute.For<ILogger<GetRecipeHistoryHandler>>(),
            _store
        );
    }

    private static IReadOnlyList<IEvent> BuildEventStream(Guid streamId)
    {
        var created = new RecipeCreated(
            streamId,
            "Chocolate Cake",
            "https://example.com/cake",
            [new Step("Mix and bake.")],
            [new Ingredient(4m, "tablespoons", "granulated sugar")],
            "Best served warm.",
            DateTimeOffset.UtcNow
        );
        var titleChanged = new RecipeTitleChanged("Updated Title");

        return
        [
            BuildEvent(streamId, 1, created, "recipe_created"),
            BuildEvent(Guid.NewGuid(), 2, titleChanged, "recipe_title_changed"),
        ];
    }

    private static IEvent BuildEvent(Guid id, long version, object data, string typeName)
    {
        var e = Substitute.For<IEvent>();
        e.Id.Returns(id);
        e.Version.Returns(version);
        e.Timestamp.Returns(DateTimeOffset.UtcNow);
        e.Data.Returns(data);
        e.EventTypeName.Returns(typeName);
        return e;
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnChronologicalEventTimeline()
    {
        // Arrange
        var ct = CancellationToken.None;
        var id = Guid.NewGuid();
        var events = BuildEventStream(id);
        _events
            .FetchStreamStateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StreamState?>(new StreamState { Id = id, Version = 2 }));
        _events
            .FetchStreamAsync(Arg.Any<Guid>(), 0L, null, 0L, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<IEvent>>(events));

        var query = new GetRecipeHistoryInput(new Id<Recipe>(id.ToString()));

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.ShouldNotBeNull();
        result.RecipeId.ShouldBe(query.RecipeId);
        result.Events.Count.ShouldBe(2);

        result.Events[0].ShouldBeOfType<RecipeHistoryEventResponse>();
        result.Events[0].Version.ShouldBe(1);
        result.Events[0].EventType.ShouldBe("recipe_created");
        result.Events[0].Timestamp.ShouldBe(events[0].Timestamp);

        result.Events[1].Version.ShouldBe(2);
        result.Events[1].EventType.ShouldBe("recipe_title_changed");
        result.Events[1].Timestamp.ShouldBe(events[1].Timestamp);
    }

    [Fact]
    public async Task Handle_WhenRecipeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _events
            .FetchStreamStateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StreamState?>(null));

        var query = new GetRecipeHistoryInput(new Id<Recipe>(Guid.NewGuid().ToString()));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenRecipeIdIsNotAGuid_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetRecipeHistoryInput(new Id<Recipe>("not-a-guid"));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetRecipeHistoryInput(new Id<Recipe>(Guid.NewGuid().ToString()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
