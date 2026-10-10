using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst;

public sealed class SymbolAnalystPromptTests
{
    [Fact]
    public void Compose_WhenAllDataIsPresent_ShouldRenderInstructionsAndEverySection()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot();

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldBe(
            SymbolAnalystPrompt.Instructions
                + """


                # Symbol: Armadyl godsword

                - Code: 11802
                - Market: OSRS
                - Market description: Old School RuneScape Grand Exchange.
                - Buy limit: 8
                - Alchemy values (fixed NPC sale prices, not market prices): high 750000, low 500000
                - Data as of: 2026-10-08 12:00

                ## Market Taxes

                Sales are taxed at 2% of the sell price per item (minimum 1, capped at 5000000).

                ## Latest Trade

                Price 10300000, volume 2, at 2026-10-08 11:55.

                ## Price Summary

                Margin is the high price minus the low price minus tax; ROI is margin over the low price.

                | Window | Avg | Low | High | Volume | Trades | Tax | Margin | ROI |
                |---|---|---|---|---|---|---|---|---|
                | OneDay | 10100000 | 9800000 | 10400000 | 40 | 12 | 208000 | 392000 | 4% |
                | OneWeek | no trades | | | | | | | |

                ## OneDay Chart (the period the user is viewing)

                Average 10100000, low 9800000, high 10400000, volume 40, 12 trades.

                | Start | Open | Close | Avg | Low | High | Volume |
                |---|---|---|---|---|---|---|
                | 2026-10-08 10:00 | 9900000 | 10100000 | 10000000 | 9800000 | 10200000 | 25 |
                | 2026-10-08 11:00 | 10100000 | 10300000 | 10266666.67 | 10100000 | 10400000 | 15 |

                ## Signals

                Aggregated score 0.4; 1 bullish, 0 bearish, 0 neutral. Flip favourable: yes. Merch favourable: no.

                | Signal | Value | Direction | Strength | Intents |
                |---|---|---|---|---|
                | RSI | 0.4 | Bullish | Medium | Buy, Sell |

                ### Signal Definitions

                - RSI: Relative strength index.

                ### Daily Trend (last 7 days, oldest first)

                - RSI: -0.2 → 0.4

                ## Forecast

                Baseline (latest actual day): average 10100000, low 9800000, high 10400000, volume 40.

                | Day | Avg | Low | High | Volume | Margin |
                |---|---|---|---|---|---|
                | 1 | 10200000 | 10000000 | 10500000 | 38 | 290000 |
                | 2 | 10200000 | 10000000 | 10500000 | 38 | 290000 |
                | 3 | 10200000 | 10000000 | 10500000 | 38 | 290000 |
                | 4 | 10200000 | 10000000 | 10500000 | 38 | 290000 |
                | 5 | 10200000 | 10000000 | 10500000 | 38 | 290000 |
                | 6 | 10200000 | 10000000 | 10500000 | 38 | 290000 |
                | 7 | 10600000 | 10000000 | 10500000 | 38 | 290000 |

                ## Forecast Accuracy (last 30 days)

                Bias is the mean of predicted minus actual average price; positive means forecasts ran high.

                | Horizon (days) | Evaluated | MAPE | MAE | Bias |
                |---|---|---|---|---|
                | 1 | 20 | 1.45% | 150000 | -20000 |

                ## Your Positions

                Most recent first. Price is per unit. Pending positions are planned but not yet filled.

                | Created | Side | Status | Quantity | Price | Notes |
                |---|---|---|---|---|---|
                | 2026-10-06 12:00 | Buy | Bought | 2 | 9900000 | Bought the dip |
                """
        );
    }

    [Fact]
    public void Compose_WhenForecastingIsDisabled_ShouldSayForecastingIsDisabled()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with
        {
            Market = SymbolAnalystSnapshotFactory.Market(isForecastingEnabled: false),
            Forecast = null,
            ForecastEfficacy = [],
        };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("Forecasting is disabled for this market.");
        prompt.ShouldNotContain("## Forecast Accuracy");
    }

    [Fact]
    public void Compose_WhenForecastIsMissing_ShouldSayNoForecastIsAvailable()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with
        {
            Forecast = null,
        };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("No forecast is available for this symbol.");
    }

    [Fact]
    public void Compose_WhenDataIsEmpty_ShouldRenderFallbackLines()
    {
        // Arrange
        var signals = SymbolAnalystSnapshotFactory.Signals() with
        {
            Signals = [],
        };
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with
        {
            Market = SymbolAnalystSnapshotFactory.Market() with { Taxes = new Taxes(null) },
            LatestTrade = null,
            SelectedTrades = new GetSymbolTradesResponse(0, 0, 0, 0, 0, 0, []),
            Signals = signals,
            Positions = [],
        };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("No tax is charged on sales.");
        prompt.ShouldContain("No trades recorded in the last year.");
        prompt.ShouldContain("No trades in this period.");
        prompt.ShouldContain("No signals have been computed for this symbol.");
        prompt.ShouldContain("No positions recorded.");
    }

    [Fact]
    public void Compose_WhenSignalHistoryIsEmpty_ShouldOmitDailyTrend()
    {
        // Arrange
        var history = SymbolAnalystSnapshotFactory.SignalHistory() with
        {
            Signals = [],
        };
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with { SignalHistory = history };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("### Signal Definitions");
        prompt.ShouldNotContain("### Daily Trend");
    }

    [Fact]
    public void Compose_WhenSymbolHasSubcodeAndExchange_ShouldRenderThem()
    {
        // Arrange
        var symbol = new GetSymbolResponse(
            SymbolAnalystSnapshotFactory.SymbolId,
            "AAPL",
            "Common",
            "Apple Inc.",
            SymbolAnalystSnapshotFactory.MarketId,
            new AdditionalFields(Exchange: "NASDAQ")
        );
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with { Symbol = symbol };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("- Subcode: Common");
        prompt.ShouldContain("- Exchange: NASDAQ");
        prompt.ShouldNotContain("- Buy limit:");
        prompt.ShouldNotContain("Alchemy values");
    }

    [Fact]
    public void Compose_WhenOnlyHighAlchemyIsKnown_ShouldLabelItAsANonMarketPrice()
    {
        // Arrange
        var symbol = SymbolAnalystSnapshotFactory.Symbol() with
        {
            AdditionalFields = new AdditionalFields(HighAlch: 750000),
        };
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with { Symbol = symbol };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain(
            "- Alchemy values (fixed NPC sale prices, not market prices): high 750000\n"
        );
    }

    [Fact]
    public void Compose_WhenWindowHasZeroLowPrice_ShouldNotComputeRoi()
    {
        // Arrange
        var trades = SymbolAnalystSnapshotFactory.OneDayTrades() with
        {
            MinPrice = 0,
        };
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with
        {
            Windows = [new SymbolAnalystWindow(TimeFrame.OneDay, trades)],
        };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain("| 0 | 10400000 | 40 | 12 | 208000 | 10192000 | - |");
    }
}
