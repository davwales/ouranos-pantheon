using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst;

public sealed class SymbolAnalystTests
{
    private const int ChartBuckets = 12;
    private const int MaxPositions = 5;

    private readonly IOuranosMachineLearningClient _mlClient =
        Substitute.For<IOuranosMachineLearningClient>();
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly SymbolAnalyst _assistant;

    public SymbolAnalystTests()
    {
        _assistant = new SymbolAnalyst(
            Substitute.For<ILogger<SymbolAnalyst>>(),
            _mlClient,
            _bus,
            _cache,
            Options.Create(
                new PlutusOptions
                {
                    SymbolAnalyst = new SymbolAnalystOptions(
                        "analyst-model",
                        512,
                        0.2f,
                        ReasoningEffort.High,
                        ChartBuckets,
                        MaxPositions,
                        ContextCacheMinutes: 5
                    ),
                }
            )
        );

        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market());

        _bus.InvokeAsync<GetSymbolTradesResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.SelectedTrades());

        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Signals());

        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Paged(Factory.OneDayTrades()));

        _bus.InvokeAsync<List<GetAllTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns([Factory.LatestTrade()]);

        _bus.InvokeAsync<GetSymbolSignalHistoryResponse>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.SignalHistory());

        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Paged(Factory.Forecast()));

        _bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Paged(Factory.ForecastEfficacy().ToArray()));

        _bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Paged(Factory.Positions().ToArray()));
    }

    private static PagedResponse<T> Paged<T>(params T[] items)
    {
        return new PagedResponse<T>(items, items.Length, 0, items.Length);
    }

    private static AssistantCompletionInput<SymbolAnalystContext> Input(
        TimeFrame timeFrame = TimeFrame.OneDay
    )
    {
        return new AssistantCompletionInput<SymbolAnalystContext>(
            [new AssistantMessageInput("Is this a good flip?", AssistantRole.User)],
            new SymbolAnalystContext(Factory.SymbolId, timeFrame)
        );
    }

    private static async IAsyncEnumerable<ResponseStreamChunk> AnswerStream(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        yield return new ResponseStreamChunk("Yes.", null, null);
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

    private void StubCompletion()
    {
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
            .Returns(AnswerStream());
    }

    [Fact]
    public async Task Handle_WhenSymbolExists_ShouldStreamWithConfiguredModelAndSymbolPrompt()
    {
        // Arrange
        string? sentInstructions = null;
        List<MessageDto>? sentMessages = null;
        _mlClient
            .StreamResponseAsync(
                "analyst-model",
                Arg.Do<string>(i => sentInstructions = i),
                Arg.Do<List<MessageDto>>(m => sentMessages = m),
                0.2f,
                512,
                ReasoningEffort.High,
                Arg.Any<CancellationToken>()
            )
            .Returns(AnswerStream());

        // Act
        var events = await CollectAsync(_assistant.Handle(Input()));

        // Assert
        events.ShouldBe([new AssistantContentEvent("Yes."), new AssistantDoneEvent()]);
        sentInstructions.ShouldNotBeNull();
        sentInstructions.ShouldStartWith(SymbolAnalystPrompt.Instructions);
        sentInstructions.ShouldContain("# Symbol: Armadyl godsword");
        sentInstructions.ShouldContain("| 1 | 20 | 1.45% | 150000 | -20000 |");
        sentInstructions.ShouldContain("Bought the dip");
        sentMessages.ShouldBe([new MessageDto("Is this a good flip?", RoleDto.User)]);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldQueryTheSelectedTimeFrameWithConfiguredBuckets()
    {
        // Arrange
        StubCompletion();

        // Act
        await CollectAsync(_assistant.Handle(Input(TimeFrame.FourHours)));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<GetSymbolTradesResponse>(
                new GetSymbolTradesInput(Factory.SymbolId, TimeFrame.FourHours, ChartBuckets),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenAskedAgainForTheSameContext_ShouldReuseTheCachedPrompt()
    {
        // Arrange
        var sentInstructions = new List<string>();
        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Do<string>(sentInstructions.Add),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_ => AnswerStream());

        // Act
        await CollectAsync(_assistant.Handle(Input()));
        await CollectAsync(_assistant.Handle(Input()));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
        sentInstructions.Count.ShouldBe(2);
        sentInstructions[1].ShouldBe(sentInstructions[0]);
    }

    [Fact]
    public async Task Handle_WhenTimeFrameChanges_ShouldLoadFreshContext()
    {
        // Arrange
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

        // Act
        await CollectAsync(_assistant.Handle(Input(TimeFrame.OneDay)));
        await CollectAsync(_assistant.Handle(Input(TimeFrame.OneWeek)));

        // Assert
        await _bus.Received(2)
            .InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldQueryEverySummaryWindowForTheSymbol()
    {
        // Arrange
        StubCompletion();
        string[] filter = [$"SymbolId:eq:{Factory.SymbolId}"];

        // Act
        await CollectAsync(_assistant.Handle(Input()));

        // Assert
        foreach (var timeFrame in SymbolAnalyst.Windows)
        {
            await _bus.Received(1)
                .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                    ArgExtensions.IsEquivalent(
                        new GetMarketTradesInput(
                            Factory.MarketId,
                            timeFrame,
                            Take: 1,
                            Filter: filter
                        )
                    ),
                    Arg.Any<CancellationToken>()
                );
        }
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldQueryLatestTradeAndRecentPositionsForTheSymbol()
    {
        // Arrange
        StubCompletion();
        string[] filter = [$"SymbolId:eq:{Factory.SymbolId}"];

        // Act
        await CollectAsync(_assistant.Handle(Input()));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<List<GetAllTradesResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetAllTradesInput(
                        TimeFrame.OneYear,
                        "Timestamp",
                        "desc",
                        Take: 1,
                        Filter: filter
                    )
                ),
                Arg.Any<CancellationToken>()
            );

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetAllPositionsInput(
                        Factory.MarketId,
                        SortField: "CreatedAt",
                        SortDirection: "desc",
                        Take: MaxPositions,
                        Filter: filter
                    )
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldStartContextQueriesConcurrently()
    {
        // Arrange
        StubCompletion();

        var signalsGate = new TaskCompletionSource<GetSymbolSignalsResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(signalsGate.Task);

        _bus.InvokeAsync<PagedResponse<GetAllPositionsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(_ =>
            {
                signalsGate.TrySetResult(Factory.Signals());
                return Paged(Factory.Positions().ToArray());
            });

        // Act
        var events = await CollectAsync(_assistant.Handle(Input()))
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        events.ShouldBe([new AssistantContentEvent("Yes."), new AssistantDoneEvent()]);
    }

    [Fact]
    public async Task Handle_WhenForecastingIsEnabled_ShouldQueryForecastAndEfficacyForTheSymbol()
    {
        // Arrange
        StubCompletion();
        string[] filter = [$"SymbolId:eq:{Factory.SymbolId}"];
        var before = DateTimeOffset.UtcNow;
        GetForecastEfficacyInput? efficacyInput = null;

        _bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(
                Arg.Do<object>(input => efficacyInput = input as GetForecastEfficacyInput),
                Arg.Any<CancellationToken>()
            )
            .Returns(Paged(Factory.ForecastEfficacy().ToArray()));

        // Act
        await CollectAsync(_assistant.Handle(Input()));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetMarketForecastInput(Factory.MarketId, Take: 1, Filter: filter)
                ),
                Arg.Any<CancellationToken>()
            );

        efficacyInput.ShouldNotBeNull();
        efficacyInput.SymbolId.ShouldBe(Factory.SymbolId.Value);
        efficacyInput.Take.ShouldBe(50);
        efficacyInput.Since.ShouldNotBeNull();
        efficacyInput.Since.Value.ShouldBeGreaterThanOrEqualTo(
            before - SymbolAnalyst.ForecastEfficacyWindow
        );
    }

    [Fact]
    public async Task Handle_WhenForecastingIsDisabled_ShouldNotQueryForecasts()
    {
        // Arrange
        string? sentInstructions = null;

        _mlClient
            .StreamResponseAsync(
                Arg.Any<string>(),
                Arg.Do<string>(i => sentInstructions = i),
                Arg.Any<List<MessageDto>>(),
                Arg.Any<float?>(),
                Arg.Any<int?>(),
                Arg.Any<ReasoningEffort?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(AnswerStream());

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market(isForecastingEnabled: false));

        // Act
        await CollectAsync(_assistant.Handle(Input()));

        // Assert
        await _bus.DidNotReceive()
            .InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            );
        await _bus.DidNotReceive()
            .InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            );
        sentInstructions.ShouldNotBeNull();
        sentInstructions.ShouldContain("Forecasting is disabled for this market.");
    }

    [Fact]
    public async Task Handle_WhenSymbolNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns<GetSymbolResponse>(_ =>
                throw new NotFoundException(Factory.SymbolId.Value, nameof(Symbol))
            );

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => CollectAsync(_assistant.Handle(Input())));
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new SymbolAnalyst(
                Substitute.For<ILogger<SymbolAnalyst>>(),
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
            new SymbolAnalyst(
                Substitute.For<ILogger<SymbolAnalyst>>(),
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
            new SymbolAnalyst(
                Substitute.For<ILogger<SymbolAnalyst>>(),
                _mlClient,
                _bus,
                _cache,
                null!
            )
        );
    }
}
