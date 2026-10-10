using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Tests.Utils;

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


                ## Symbol: Armadyl godsword
                - Code: 11802
                - Market: OSRS (Old School RuneScape Grand Exchange.)
                - Buy limit: 8
                - Alchemy values (fixed NPC sale prices, not market prices): high 750K, low 500K
                - Tax: 2% of the sell price per item (min 1, max 5M)
                - Latest trade: 10.3M × 2 at 2026-10-08 11:55
                - Data as of: 2026-10-08 12:00

                ## Price Summary (Margin = high - low - tax; ROI = margin / low)
                Window | Avg | Low | High | Vol | Margin | ROI
                OneDay | 10.1M | 9.8M | 10.4M | 40 | 392K | 4%
                OneWeek | no trades

                ## OneDay Chart (the period the user is viewing)
                Avg 10.1M, low 9.8M, high 10.4M, vol 40, 12 trades; open 9.9M → close 10.3M (+4%).
                Start | Avg | Low | High | Vol
                10-08 10:00 | 10M | 9.8M | 10.2M | 25
                10-08 11:00 | 10.27M | 10.1M | 10.4M | 15

                ## Signals (7d trend: oldest → latest daily value)
                Score 0.4 (1 bullish, 0 bearish, 0 neutral); flip favourable: yes; merch favourable: no.
                - RSI: 0.4 Bullish Medium (Buy, Sell); 7d -0.2 → 0.4

                ## Forecast (baseline day: avg 10.1M, low 9.8M, high 10.4M, vol 40)
                Day | Avg | Low | High | Margin
                1 | 10.2M | 10M | 10.5M | 290K
                3 | 10.2M | 10M | 10.5M | 290K
                7 | 10.6M | 10M | 10.5M | 290K
                Error (MAPE, last 30d, n=20): 1d 1.5%; forecasts ran low.

                ## Your Positions (most recent first; price per unit; Pending = planned, not filled)
                Created | Side | Status | Qty | Price | Notes
                2026-10-06 | Buy | Bought | 2 | 9.9M | Bought the dip
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
        prompt.ShouldNotContain("Error (MAPE");
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
        prompt.ShouldContain("- Tax: none");
        prompt.ShouldContain("- Latest trade: none in the last year");
        prompt.ShouldContain("No trades in this period.");
        prompt.ShouldContain("No signals have been computed for this symbol.");
        prompt.ShouldContain("No positions recorded.");
    }

    [Fact]
    public void Compose_WhenSignalHistoryIsEmpty_ShouldOmitTheTrend()
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
        prompt.ShouldContain("- RSI: 0.4 Bullish Medium (Buy, Sell)\n");
        prompt.ShouldNotContain("; 7d");
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
        prompt.ShouldContain("- Code: AAPL (Common)");
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
            "- Alchemy values (fixed NPC sale prices, not market prices): high 750K\n"
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
        prompt.ShouldContain("OneDay | 10.1M | 0 | 10.4M | 40 | 10.19M | -\n");
    }

    [Fact]
    public void Compose_WhenSnapshotIsFull_ShouldStayWithinTheContextBudget()
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Full();

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        (
            prompt.Length + "\n\n".Length + AssistantFormatting.Instructions.Length
        ).ShouldBeLessThanOrEqualTo(AssistantPromptBudget.Characters);
    }

    [Theory]
    [MemberData(nameof(TimeFrames))]
    public void Compose_ForEveryTimeFrame_ShouldCapTheChartAtTheConfiguredPoints(
        TimeFrame timeFrame
    )
    {
        // Arrange
        var snapshot = SymbolAnalystSnapshotFactory.Full(timeFrame);

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        var chart = prompt[
            prompt.IndexOf("Start | Avg | Low | High | Vol\n", StringComparison.Ordinal)..
        ];
        var rows = chart.Split("\n\n")[0].Split('\n').Skip(1);
        rows.Count().ShouldBe(SymbolAnalystSnapshotFactory.ChartPoints);
    }

    public static TheoryData<TimeFrame> TimeFrames()
    {
        return [.. Enum.GetValues<TimeFrame>()];
    }

    [Fact]
    public void Downsample_WhenThereAreMoreBucketsThanPoints_ShouldMergeTradedBucketsByVolume()
    {
        // Arrange
        var start = SymbolAnalystSnapshotFactory.GeneratedAt;
        List<GetSymbolTradeBucketsResponse> buckets =
        [
            new(100, 1, 100, 90, 110, 1, start, 95, 105),
            new(200, 3, 600, 150, 250, 2, start.AddHours(1), 105, 210),
            new(300, 0, 0, 0, 0, 0, start.AddHours(2), 0, 0),
            new(400, 2, 800, 350, 450, 1, start.AddHours(3), 380, 420),
        ];

        // Act
        var points = SymbolAnalystPrompt.Downsample(buckets, 2);

        // Assert
        points.ShouldBe([
            new GetSymbolTradeBucketsResponse(175, 4, 700, 90, 250, 3, start, 95, 210),
            new GetSymbolTradeBucketsResponse(
                400,
                2,
                800,
                350,
                450,
                1,
                start.AddHours(2),
                380,
                420
            ),
        ]);
    }

    [Fact]
    public void Compose_WhenEfficacySpansSeveralModels_ShouldShowTheMostEvaluatedModelOnce()
    {
        // Arrange
        var row = SymbolAnalystSnapshotFactory.ForecastEfficacy()[0];
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with
        {
            ForecastEfficacy =
            [
                row with
                {
                    ModelName = "old",
                    EvaluatedCount = 2,
                    MeanAbsolutePercentageError = 0.5m,
                },
                row with
                {
                    HorizonDays = 7,
                    MeanAbsolutePercentageError = 0.04m,
                    MeanBias = 10,
                },
                row with
                {
                    HorizonDays = 3,
                    MeanAbsolutePercentageError = 0.02m,
                },
                row with
                {
                    HorizonDays = 2,
                    MeanAbsolutePercentageError = 0.9m,
                },
                row,
            ],
        };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain(
            "Error (MAPE, last 30d, n=20): 1d 1.5%, 3d 2%, 7d 4%; forecasts ran low.\n"
        );
    }

    [Fact]
    public void Compose_WhenPositionNoteIsLong_ShouldTruncateItToOneLine()
    {
        // Arrange
        var position = SymbolAnalystSnapshotFactory.Positions()[0] with
        {
            Notes = "Line one\n" + new string('x', 100),
        };
        var snapshot = SymbolAnalystSnapshotFactory.Snapshot() with { Positions = [position] };

        // Act
        var prompt = SymbolAnalystPrompt.Compose(snapshot);

        // Assert
        prompt.ShouldContain(
            $"| Line one {new string('x', SymbolAnalystPrompt.MaxNoteLength - 10)}…"
        );
    }
}
