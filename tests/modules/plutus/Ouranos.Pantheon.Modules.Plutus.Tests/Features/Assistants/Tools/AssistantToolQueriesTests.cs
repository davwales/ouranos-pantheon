using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering.Schemas;
using Ouranos.Pantheon.Tests.Utils.NSubstitute;
using Wolverine;
using Factory = Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.AskSymbolAnalyst.SymbolAnalystSnapshotFactory;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Assistants.Tools;

public sealed class AssistantToolQueriesTests
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
        return AssistantToolQueries.LoadPriceSummaryAsync(
            _bus,
            Factory.MarketId,
            timeFrame,
            Factory.SymbolId,
            CancellationToken.None
        );
    }

    [Fact]
    public void SymbolFilter_ShouldMatchTheSymbolIdExactly()
    {
        // Act
        var filter = AssistantToolQueries.SymbolFilter(Factory.SymbolId);

        // Assert
        filter.Length.ShouldBe(1);
        FilterParser
            .Parse(filter[0])
            .ShouldBe(
                new FieldFilterNode(
                    nameof(GetMarketTradesResponse.SymbolId),
                    FilterOperator.Eq,
                    Factory.SymbolId.Value
                )
            );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsForecastingEnabledAsync_ShouldReadTheMarketsSetting(bool isEnabled)
    {
        // Arrange
        _bus.InvokeAsync<GetMarketResponse>(
                ArgExtensions.IsEquivalent(new GetMarketInput(Factory.MarketId)),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Market() with { IsForecastingEnabled = isEnabled });

        // Act
        var result = await AssistantToolQueries.IsForecastingEnabledAsync(
            _bus,
            Factory.MarketId,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(isEnabled);
    }

    [Fact]
    public async Task LoadForecastAsync_WhenTheSymbolHasAForecast_ShouldReturnTheLatest()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                ArgExtensions.IsEquivalent(
                    new GetMarketForecastInput(
                        Factory.MarketId,
                        Take: 1,
                        Filter: AssistantToolQueries.SymbolFilter(Factory.SymbolId)
                    )
                ),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged(Factory.Forecast()));

        // Act
        var result = await AssistantToolQueries.LoadForecastAsync(
            _bus,
            Factory.MarketId,
            Factory.SymbolId,
            CancellationToken.None
        );

        // Assert
        result.ShouldBe(Factory.Forecast());
    }

    [Fact]
    public async Task LoadForecastAsync_WhenTheSymbolHasNoForecast_ShouldReturnNull()
    {
        // Arrange
        _bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Factory.Paged<GetMarketForecastResponse>());

        // Act
        var result = await AssistantToolQueries.LoadForecastAsync(
            _bus,
            Factory.MarketId,
            Factory.SymbolId,
            CancellationToken.None
        );

        // Assert
        result.ShouldBeNull();
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
