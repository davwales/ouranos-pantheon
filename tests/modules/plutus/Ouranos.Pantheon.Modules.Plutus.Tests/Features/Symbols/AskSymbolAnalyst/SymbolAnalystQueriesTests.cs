using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst;

public sealed class SymbolAnalystQueriesTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    private void ReturnTrades(params GetMarketTradesResponse[] trades)
    {
        _bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(trades));
    }

    private Task<PriceSummaryData> LoadPriceSummaryAsync(TimeFrame timeFrame)
    {
        return SymbolAnalystQueries.LoadPriceSummaryAsync(
            _bus,
            Factory.MarketId,
            timeFrame,
            Factory.SymbolId,
            CancellationToken.None
        );
    }

    [Fact]
    public async Task LoadSymbolAsync_WhenSymbolExists_ShouldLoadItsMarket()
    {
        // Arrange
        _bus.InvokeAsync<GetSymbolResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Factory.Symbol());

        _bus.InvokeAsync<GetMarketResponse>(
                ArgExtensions.IsEquivalent(new GetMarketInput(Factory.MarketId)),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Market());

        // Act
        var (symbol, market) = await SymbolAnalystQueries.LoadSymbolAsync(
            _bus,
            Factory.SymbolId,
            CancellationToken.None
        );

        // Assert
        symbol.ShouldBe(Factory.Symbol());
        market.ShouldBe(Factory.Market());
    }

    [Fact]
    public async Task LoadPriceSummaryAsync_WhenThereAreTrades_ShouldIncludeMarginAndRoiInPercent()
    {
        // Arrange
        ReturnTrades(Factory.OneDayTrades());

        // Act
        var summary = await LoadPriceSummaryAsync(TimeFrame.OneDay);

        // Assert
        summary.ShouldBe(
            new PriceSummaryData(
                TimeFrame.OneDay,
                HasTrades: true,
                AveragePrice: 10100000,
                LowPrice: 9800000,
                HighPrice: 10400000,
                Volume: 40,
                Trades: 12,
                MarginAfterTax: 392000,
                RoiPercent: 4
            )
        );

        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetMarketTradesInput(
                        Factory.MarketId,
                        TimeFrame.OneDay,
                        Take: 1,
                        Filter: [$"SymbolId:eq:{Factory.SymbolId}"]
                    )
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task LoadPriceSummaryAsync_WhenThereAreNoTrades_ShouldOnlySaySo()
    {
        // Arrange
        ReturnTrades();

        // Act
        var summary = await LoadPriceSummaryAsync(TimeFrame.OneWeek);

        // Assert
        summary.ShouldBe(new PriceSummaryData(TimeFrame.OneWeek, HasTrades: false));
    }

    [Fact]
    public async Task LoadPriceSummaryAsync_WhenLowPriceIsZero_ShouldNotComputeRoi()
    {
        // Arrange
        ReturnTrades(Factory.OneDayTrades() with { MinPrice = 0 });

        // Act
        var summary = await LoadPriceSummaryAsync(TimeFrame.OneDay);

        // Assert
        summary.RoiPercent.ShouldBeNull();
        summary.MarginAfterTax.ShouldBe(10192000);
    }
}
