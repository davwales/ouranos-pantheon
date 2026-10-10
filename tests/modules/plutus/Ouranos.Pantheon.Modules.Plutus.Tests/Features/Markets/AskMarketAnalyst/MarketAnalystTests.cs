using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst.MarketAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst;

public sealed class MarketAnalystTests
{
    private const int MaxViewRows = 5;

    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly MarketAnalyst _assistant;

    public MarketAnalystTests()
    {
        _assistant = new MarketAnalyst(
            Substitute.For<ILogger<MarketAnalyst>>(),
            _mlClient,
            _bus,
            _cache,
            Options.Create(
                new PlutusOptions
                {
                    MarketAnalyst = new MarketAnalystOptions(
                        "analyst-model",
                        512,
                        0.2f,
                        ReasoningEffort.Low,
                        MaxViewRows,
                        TrendPoints: 2,
                        ContextCacheMinutes: 5
                    ),
                }
            )
        );

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market());

        _bus.InvokeAsync<GetMarketOverviewResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Overview());

        _bus.InvokeAsync<GetVolumeHeatmapResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Heatmap());

        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.View(Factory.TwistedBow(), Factory.RunePlatebody()));

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_ => AnswerStream());
    }

    private static AssistantCompletionInput<MarketAnalystContext> Input(
        MarketAnalystContext? context = null
    )
    {
        return new AssistantCompletionInput<MarketAnalystContext>(
            [new AssistantMessageInput("Which of these is best?", AssistantRole.User)],
            context ?? Factory.Context()
        );
    }

    private static async IAsyncEnumerable<ResponseStreamChunk> AnswerStream(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        yield return new ResponseStreamChunk("Twisted bow.", null, null);
    }

    private static async Task<List<AssistantEvent>> CollectAsync(
        IAsyncEnumerable<AssistantEvent> stream
    )
    {
        var events = new List<AssistantEvent>();
        await foreach (var assistantEvent in stream)
        {
            events.Add(assistantEvent);
        }

        return events;
    }

    [Fact]
    public async Task Handle_WhenMarketExists_ShouldStreamWithConfiguredModelAndMarketPrompt()
    {
        // Arrange
        string? sentInstructions = null;
        _mlClient
            .StreamResponseAsync(
                "analyst-model",
                Arg.Do<string>(i => sentInstructions = i),
                Arg.Any<List<MessageDto>>(),
                0.2f,
                512,
                ReasoningEffort.Low,
                Arg.Any<CancellationToken>()
            )
            .Returns(_ => AnswerStream());

        // Act
        var events = await CollectAsync(_assistant.Handle(Input()));

        // Assert
        events.ShouldBe([new AssistantContentEvent("Twisted bow."), new AssistantDoneEvent()]);
        sentInstructions.ShouldNotBeNull();
        sentInstructions.ShouldStartWith(MarketAnalystPrompt.Instructions);
        sentInstructions.ShouldContain("## Market: OSRS");
        sentInstructions.ShouldContain("Twisted bow | 1.4B");
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldQueryTheUsersViewWithTakeCappedToMaxViewRows()
    {
        // Arrange
        var context = Factory.Context() with
        {
            Take = 50,
        };

        // Act
        await CollectAsync(_assistant.Handle(Input(context)));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Is<GetMarketTradesInput>(i =>
                    i.MarketId == Factory.MarketId
                    && i.TimeFrame == TimeFrame.OneDay
                    && i.SortField == "Roi"
                    && i.SortDirection == "desc"
                    && i.Skip == 10
                    && i.Take == MaxViewRows
                    && i.Filter != null
                    && i.Filter.SequenceEqual(new[] { "TotalVolume:gte:2" })
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldQueryOverviewForTheTimeFrameAndRecentActivity()
    {
        // Arrange
        var context = new MarketAnalystContext(Factory.MarketId, TimeFrame.OneWeek);

        // Act
        await CollectAsync(_assistant.Handle(Input(context)));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<GetMarketOverviewResponse>(
                new GetMarketOverviewInput(Factory.MarketId, TimeFrame.OneWeek),
                Arg.Any<CancellationToken>()
            );

        await _bus.Received(1)
            .InvokeAsync<GetVolumeHeatmapResponse>(
                new GetVolumeHeatmapInput(Factory.MarketId, MarketAnalyst.HeatmapLookbackWeeks),
                Arg.Any<CancellationToken>()
            );

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Is<GetMarketTradesInput>(i =>
                    i.SortField == null && i.Filter == null && i.Skip == 0
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenAskedAgainForTheSameView_ShouldReuseTheCachedPrompt()
    {
        // Act
        await CollectAsync(_assistant.Handle(Input()));
        await CollectAsync(_assistant.Handle(Input()));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheViewChanges_ShouldReloadTheContext()
    {
        // Arrange
        var filtered = Factory.Context() with
        {
            Filter = ["SymbolName:like:rune"],
        };

        // Act
        await CollectAsync(_assistant.Handle(Input()));
        await CollectAsync(_assistant.Handle(Input(filtered)));

        // Assert
        await _bus.Received(2)
            .InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMarketNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns<GetMarketResponse>(_ =>
                throw new NotFoundException(Factory.MarketId.Value, nameof(Market))
            );

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => CollectAsync(_assistant.Handle(Input())));
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new MarketAnalyst(
                Substitute.For<ILogger<MarketAnalyst>>(),
                _mlClient,
                null!,
                _cache,
                Options.Create(new PlutusOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenCacheIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new MarketAnalyst(
                Substitute.For<ILogger<MarketAnalyst>>(),
                _mlClient,
                _bus,
                null!,
                Options.Create(new PlutusOptions())
            )
        );
    }

    [Fact]
    public void Constructor_WhenOptionsAreNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new MarketAnalyst(
                Substitute.For<ILogger<MarketAnalyst>>(),
                _mlClient,
                _bus,
                _cache,
                null!
            )
        );
    }
}
