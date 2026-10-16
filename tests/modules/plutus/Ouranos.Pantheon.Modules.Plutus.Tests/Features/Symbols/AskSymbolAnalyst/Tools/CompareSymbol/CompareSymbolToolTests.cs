using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.CompareSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Tools.CompareSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.Tools.CompareSymbol;

public sealed class CompareSymbolToolTests
{
    private static readonly SymbolAnalystContext Context = new(Factory.SymbolId, TimeFrame.OneDay);

    private static readonly Id<Symbol> OtherId = new("8c1f7f1e-0000-4000-8000-000000000003");

    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly CompareSymbolTool _tool;

    public CompareSymbolToolTests()
    {
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(Factory.OneDayTrades()));

        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Signals());

        _tool = new CompareSymbolTool(Substitute.For<ILogger<CompareSymbolTool>>(), _bus);
    }

    private Task<AssistantToolResult> InvokeAsync(string arguments)
    {
        return _tool.InvokeAsync(Context, arguments, CancellationToken.None).AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsInTheMarket_ShouldReturnTheOtherSymbolsSummaries()
    {
        // Arrange
        ReturnSymbolFor(OtherId, OtherSymbol(Factory.MarketId));

        // Act
        var result = await InvokeAsync($$"""{"symbolId":"{{OtherId}}"}""");

        // Assert
        var data = result.ShouldContainData<ComparedSymbolData>();
        data.Name.ShouldBe("Bandos godsword");

        data.PriceSummaries.Select(summary => summary.TimeFrame)
            .ShouldBe(SymbolAnalystQueries.SummaryTimeFrames);

        data.PriceSummaries[0].MarginAfterTax.ShouldBe(392000);
        data.SignalScore.AggregatedScore.ShouldBe(0.4m);
        data.StrongestSignals.Single().Name.ShouldBe("RSI");

        await _bus.Received(1)
            .InvokeAsync<GetSymbolSignalsResponse>(
                ArgExtensions.IsEquivalent(new GetSymbolSignalsInput(OtherId)),
                Arg.Any<CancellationToken>()
            );

        await _bus.DidNotReceive()
            .InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreManySignals_ShouldReturnOnlyTheStrongest()
    {
        // Arrange
        ReturnSymbolFor(OtherId, OtherSymbol(Factory.MarketId));
        var rsi = Factory.Signals().Signals[0];

        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(
                Factory.Signals() with
                {
                    Signals =
                    [
                        rsi with
                        {
                            Label = "Weak",
                            Value = 0.1m,
                        },
                        rsi with
                        {
                            Label = "Bearish",
                            Value = -0.9m,
                        },
                        rsi with
                        {
                            Label = "Middle",
                            Value = 0.5m,
                        },
                        rsi with
                        {
                            Label = "Strong",
                            Value = 0.7m,
                        },
                    ],
                }
            );

        // Act
        var result = await InvokeAsync($$"""{"symbolId":"{{OtherId}}"}""");

        // Assert
        result
            .ShouldContainData<ComparedSymbolData>()
            .StrongestSignals.Select(signal => signal.Name)
            .ShouldBe(["Bearish", "Strong", "Middle"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsTheCurrentSymbol_ShouldSaySo()
    {
        // Act
        var result = await InvokeAsync($$"""{"symbolId":"{{Factory.SymbolId}}"}""");

        // Assert
        result.ShouldContainNote().ShouldBe(CompareSymbolTool.SameSymbol);
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsUnknown_ShouldPointToSearch()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolResponse>(
                ArgExtensions.IsEquivalent(new GetSymbolInput(OtherId)),
                Arg.Any<CancellationToken>()
            )
            .Returns<GetSymbolResponse>(_ =>
                throw new NotFoundException(OtherId.Value, nameof(Symbol))
            );

        // Act
        var result = await InvokeAsync($$"""{"symbolId":"{{OtherId}}"}""");

        // Assert
        result.ShouldContainNote().ShouldBe(CompareSymbolTool.NotInMarketNote(OtherId));
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsInAnotherMarket_ShouldPointToSearch()
    {
        // Arrange
        ReturnSymbolFor(
            OtherId,
            OtherSymbol(new Id<Market>("8c1f7f1e-0000-4000-8000-000000000009"))
        );

        // Act
        var result = await InvokeAsync($$"""{"symbolId":"{{OtherId}}"}""");

        // Assert
        result.ShouldContainNote().ShouldBe(CompareSymbolTool.NotInMarketNote(OtherId));

        await _bus.DidNotReceive()
            .InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new CompareSymbolTool(Substitute.For<ILogger<CompareSymbolTool>>(), null!)
        );
    }

    private static GetSymbolResponse OtherSymbol(Id<Market> marketId)
    {
        return Factory.Symbol() with
        {
            Id = OtherId,
            Code = "11804",
            Name = "Bandos godsword",
            MarketId = marketId,
        };
    }

    private void ReturnSymbolFor(Id<Symbol> symbolId, GetSymbolResponse symbol)
    {
        _bus.InvokeAsync<GetSymbolResponse>(
                ArgExtensions.IsEquivalent(new GetSymbolInput(symbolId)),
                Arg.Any<CancellationToken>()
            )
            .Returns(symbol);
    }
}
