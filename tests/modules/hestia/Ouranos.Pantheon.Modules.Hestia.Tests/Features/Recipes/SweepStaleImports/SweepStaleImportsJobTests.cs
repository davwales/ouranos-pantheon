using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.SweepStaleImports;
using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using IEventStoreOperations = Marten.Events.IEventStoreOperations;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Recipes.SweepStaleImports;

public sealed class SweepStaleImportsJobTests
{
    private static readonly DateTimeOffset Cutoff = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    private readonly IHestiaMartenStore _store = Substitute.For<IHestiaMartenStore>();
    private readonly IDocumentSession _session = Substitute.For<IDocumentSession>();
    private readonly IEventStoreOperations _events = Substitute.For<IEventStoreOperations>();
    private readonly IEventStream<Recipe> _stream = Substitute.For<IEventStream<Recipe>>();
    private readonly SweepStaleImportsJob _job;

    public SweepStaleImportsJobTests()
    {
        _session.Events.Returns(_events);
        _session.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _store.LightweightSession().Returns(_session);
        _events
            .FetchForWriting<Recipe>(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_stream));

        _job = new SweepStaleImportsJob(
            Substitute.For<ILogger<SweepStaleImportsJob>>(),
            _store,
            Options.Create(new HestiaOptions())
        );
    }

    private static Recipe Importing(DateTimeOffset startedAt)
    {
        return Recipe.CreateImport(Guid.NewGuid(), "https://example.com/cake", startedAt).State;
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SweepStaleImportsJob(
                Substitute.For<ILogger<SweepStaleImportsJob>>(),
                null!,
                Options.Create(new HestiaOptions())
            )
        );
    }

    [Fact]
    public void Stale_WhenImportStartedBeforeTheCutoff_ShouldMatch()
    {
        // Arrange
        var recipe = Importing(Cutoff.AddMinutes(-1));

        // Act & Assert
        SweepStaleImportsJob.Stale(Cutoff).Compile()(recipe).ShouldBeTrue();
    }

    [Fact]
    public void Stale_WhenImportStartedAfterTheCutoff_ShouldNotMatch()
    {
        // Arrange
        var recipe = Importing(Cutoff.AddMinutes(1));

        // Act & Assert
        SweepStaleImportsJob.Stale(Cutoff).Compile()(recipe).ShouldBeFalse();
    }

    [Fact]
    public void Stale_WhenImportingWithoutAStartTime_ShouldMatch()
    {
        // Arrange
        var recipe = Importing(Cutoff) with
        {
            ImportStartedAt = null,
        };

        // Act & Assert
        SweepStaleImportsJob.Stale(Cutoff).Compile()(recipe).ShouldBeTrue();
    }

    [Theory]
    [InlineData(RecipeImportStatus.None)]
    [InlineData(RecipeImportStatus.Imported)]
    [InlineData(RecipeImportStatus.Failed)]
    public void Stale_WhenNotImporting_ShouldNotMatch(RecipeImportStatus status)
    {
        // Arrange
        var recipe = Importing(Cutoff.AddDays(-1)) with
        {
            ImportStatus = status,
        };

        // Act & Assert
        SweepStaleImportsJob.Stale(Cutoff).Compile()(recipe).ShouldBeFalse();
    }

    [Fact]
    public async Task TryFailAsync_WhenImportIsStale_ShouldAppendImportFailedAndSave()
    {
        // Arrange
        var recipe = Importing(Cutoff.AddHours(-1));
        _stream.Aggregate.Returns(recipe);
        object[]? appended = null;
        _stream
            .When(s => s.AppendMany(Arg.Any<object[]>()))
            .Do(call => appended = call.Arg<object[]>());

        // Act
        var failed = await _job.TryFailAsync(recipe.Id, Cutoff, CancellationToken.None);

        // Assert
        failed.ShouldBeTrue();
        var @event = appended.ShouldHaveSingleItem().ShouldBeOfType<RecipeImportFailed>();
        @event.Reason.ShouldBe(RecipeImportFailureReasons.Stale);
        await _session.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryFailAsync_WhenImportFinishedSinceTheQuery_ShouldLeaveItAlone()
    {
        // Arrange
        var recipe = Importing(Cutoff.AddHours(-1)) with
        {
            ImportStatus = RecipeImportStatus.Imported,
        };
        _stream.Aggregate.Returns(recipe);

        // Act
        var failed = await _job.TryFailAsync(recipe.Id, Cutoff, CancellationToken.None);

        // Assert
        failed.ShouldBeFalse();
        _stream.DidNotReceive().AppendMany(Arg.Any<object[]>());
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryFailAsync_WhenRecipeNoLongerExists_ShouldReturnFalse()
    {
        // Arrange
        _stream.Aggregate.Returns((Recipe?)null);

        // Act
        var failed = await _job.TryFailAsync(Guid.NewGuid(), Cutoff, CancellationToken.None);

        // Assert
        failed.ShouldBeFalse();
        await _session.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryFailAsync_WhenTheStreamMovedOnBeforeSaving_ShouldReturnFalse()
    {
        // Arrange
        var recipe = Importing(Cutoff.AddHours(-1));
        _stream.Aggregate.Returns(recipe);
        _session
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException(
                    new EventStreamUnexpectedMaxEventIdException(recipe.Id, typeof(Recipe), 2, 3)
                )
            );

        // Act
        var failed = await _job.TryFailAsync(recipe.Id, Cutoff, CancellationToken.None);

        // Assert
        failed.ShouldBeFalse();
    }
}
