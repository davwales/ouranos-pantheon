using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst;

public sealed class SymbolAnalystTests
{
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
                        ContextCacheMinutes: 5
                    ),
                }
            ),
            []
        );

        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market());

        _bus.InvokeAsync<GetDailySymbolSummaryResponse>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Today());
    }

    private static AssistantCompletionInput<SymbolAnalystContext> Input(
        TimeFrame timeFrame = TimeFrame.OneDay
    )
    {
        return new AssistantCompletionInput<SymbolAnalystContext>(
            [new AssistantMessageInput("Is this a good flip?", AssistantRole.User)],
            new SymbolAnalystContext(Factory.MarketId, Factory.SymbolId, timeFrame)
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
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
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
        IReadOnlyList<ResponseInputItemDto>? sentMessages = null;
        _mlClient
            .StreamResponseAsync(
                "analyst-model",
                Arg.Do<string>(i => sentInstructions = i),
                Arg.Do<IReadOnlyList<ResponseInputItemDto>>(m => sentMessages = m),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
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
        sentInstructions.ShouldContain(
            "## Today So Far\nAvg 10.1M, low 9.8M, high 10.4M, vol 40.\n"
        );
        sentMessages.ShouldBe([
            new MessageInputItemDto(new MessageDto("Is this a good flip?", RoleDto.User)),
        ]);
    }

    [Fact]
    public async Task Handle_WhenCalled_ShouldQueryOnlyTheSymbolMarketAndTodaysSummary()
    {
        // Arrange
        StubCompletion();

        // Act
        await CollectAsync(_assistant.Handle(Input()));

        // Assert
        await _bus.Received(1)
            .InvokeAsync<GetDailySymbolSummaryResponse>(
                ArgExtensions.IsEquivalent(new GetDailySymbolSummaryInput(Factory.SymbolId)),
                Arg.Any<CancellationToken>()
            );

        await _bus.Received(1)
            .InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());

        await _bus.Received(1)
            .InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());

        _bus.ReceivedCalls().Count().ShouldBe(3);
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
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
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
                Arg.Any<IReadOnlyList<ResponseInputItemDto>>(),
                Arg.Any<IReadOnlyList<FunctionToolDto>>(),
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
    public async Task Handle_WhenCalled_ShouldQueryTodaysSummaryAlongsideTheSymbol()
    {
        // Arrange
        StubCompletion();

        var todayGate = new TaskCompletionSource<GetDailySymbolSummaryResponse>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        _bus.InvokeAsync<GetDailySymbolSummaryResponse>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(todayGate.Task);

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                todayGate.TrySetResult(Factory.Today());
                return Factory.Market();
            });

        // Act
        var events = await CollectAsync(_assistant.Handle(Input()))
            .WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        events.ShouldBe([new AssistantContentEvent("Yes."), new AssistantDoneEvent()]);
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
    public async Task Handle_WhenSymbolIsInAnotherMarket_ShouldThrowNotFoundException()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(
                Factory.Symbol() with
                {
                    MarketId = new Id<Market>("8c1f7f1e-0000-4000-8000-000000000009"),
                }
            );

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => CollectAsync(_assistant.Handle(Input())));

        await _bus.DidNotReceive()
            .InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
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
                Options.Create(new PlutusOptions()),
                []
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
                Options.Create(new PlutusOptions()),
                []
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
                null!,
                []
            )
        );
    }
}
