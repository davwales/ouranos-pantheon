using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskMarketAnalyst.MarketAnalystSnapshotFactory;
using SymbolFactory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools.GetSymbol;

public sealed class GetSymbolToolTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly GetSymbolTool _tool;

    public GetSymbolToolTests()
    {
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Symbol(Factory.MarketId));

        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market());

        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.View(Factory.TwistedBow()));

        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(SymbolFactory.Signals());

        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(SymbolFactory.Paged(SymbolFactory.Forecast()));

        _tool = new GetSymbolTool(Substitute.For<ILogger<GetSymbolTool>>(), _bus);
    }

    private Task<AssistantToolResult> InvokeAsync()
    {
        return _tool
            .InvokeAsync(
                Factory.Context(),
                $$"""{"symbolId":"{{Factory.TwistedBowId}}"}""",
                CancellationToken.None
            )
            .AsTask();
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsInTheMarket_ShouldReturnSummariesSignalsAndForecast()
    {
        // Act
        var result = await InvokeAsync();

        // Assert
        var data = result.ShouldContainData<MarketSymbolData>();
        data.Name.ShouldBe("Twisted bow");

        data.PriceSummaries.Select(summary => summary.TimeFrame)
            .ShouldBe(AssistantToolQueries.SummaryTimeFrames);

        data.SignalScore.AggregatedScore.ShouldBe(0.4m);
        data.StrongestSignals.Single().Name.ShouldBe("RSI");

        data.Forecast.ShouldBe(
            new ForecastOutlookData(10100000, 10200000, 2320000, 10600000, 2320000)
        );
    }

    [Fact]
    public async Task InvokeAsync_WhenForecastingIsDisabled_ShouldOmitTheForecast()
    {
        // Arrange
        _bus.InvokeAsync<GetMarketResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Market() with { IsForecastingEnabled = false });

        // Act
        var result = await InvokeAsync();

        // Assert
        result.Output.ShouldNotContain("forecast");

        await _bus.DidNotReceive()
            .InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsUnknown_ShouldPointToSearch()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns<GetSymbolResponse>(_ =>
                throw new NotFoundException(Factory.TwistedBowId.Value, nameof(Symbol))
            );

        // Act
        var result = await InvokeAsync();

        // Assert
        result.ShouldContainNote().ShouldBe(GetSymbolTool.NotInMarketNote(Factory.TwistedBowId));
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsInAnotherMarket_ShouldPointToSearch()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Symbol(new Id<Market>("5a2e0c11-0000-4000-8000-000000000009")));

        // Act
        var result = await InvokeAsync();

        // Assert
        result.ShouldContainNote().ShouldBe(GetSymbolTool.NotInMarketNote(Factory.TwistedBowId));

        await _bus.DidNotReceive()
            .InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_WhenThereAreManySignals_ShouldReturnOnlyTheStrongest()
    {
        // Arrange
        var rsi = SymbolFactory.Signals().Signals[0];

        _bus.InvokeAsync<GetSymbolSignalsResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(
                SymbolFactory.Signals() with
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
        var result = await InvokeAsync();

        // Assert
        result
            .ShouldContainData<MarketSymbolData>()
            .StrongestSignals.Select(signal => signal.Name)
            .ShouldBe(["Bearish", "Strong", "Middle"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenIdIsTheAnchoredSymbol_ShouldSaySo()
    {
        // Arrange
        var context = new SymbolAnalystContext(
            Factory.MarketId,
            Factory.TwistedBowId,
            TimeFrame.OneDay
        );

        // Act
        var result = await _tool.InvokeAsync(
            context,
            $$"""{"symbolId":"{{Factory.TwistedBowId}}"}""",
            CancellationToken.None
        );

        // Assert
        result.ShouldContainNote().ShouldBe(GetSymbolTool.SameSymbol);

        await _bus.DidNotReceive()
            .InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_WhenBusIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new GetSymbolTool(Substitute.For<ILogger<GetSymbolTool>>(), null!)
        );
    }

    private static GetSymbolResponse Symbol(Id<Market> marketId)
    {
        return new GetSymbolResponse(
            Factory.TwistedBowId,
            "20997",
            null,
            "Twisted bow",
            marketId,
            new AdditionalFields(Limit: 8)
        );
    }
}
