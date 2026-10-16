using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Tools.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Assistants;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst.MarketAnalystSnapshotFactory;
using SymbolFactory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst.Tools.GetSymbol;

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
            .ShouldBe(SymbolAnalystQueries.SummaryTimeFrames);

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
    public async Task InvokeAsync_WhenIdIsUnknown_ShouldPointToFindSymbols()
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
    public async Task InvokeAsync_WhenIdIsInAnotherMarket_ShouldPointToFindSymbols()
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
