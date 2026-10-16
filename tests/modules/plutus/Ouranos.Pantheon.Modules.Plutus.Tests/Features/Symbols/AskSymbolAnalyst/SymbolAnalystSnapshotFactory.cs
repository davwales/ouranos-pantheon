using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Positions;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.AskSymbolAnalyst;

internal static class SymbolAnalystSnapshotFactory
{
    public static readonly Id<Symbol> SymbolId = new("8c1f7f1e-0000-4000-8000-000000000001");
    public static readonly Id<Market> MarketId = new("8c1f7f1e-0000-4000-8000-000000000002");
    public static readonly DateTimeOffset GeneratedAt = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);
    public const int ChartPoints = 8;

    public static PagedResponse<T> Paged<T>(params T[] items)
    {
        return new PagedResponse<T>(items, items.Length, 0, items.Length);
    }

    public static GetSymbolResponse Symbol()
    {
        return new GetSymbolResponse(
            SymbolId,
            "11802",
            null,
            "Armadyl godsword",
            MarketId,
            new AdditionalFields(Limit: 8, HighAlch: 750000, LowAlch: 500000)
        );
    }

    public static GetMarketResponse Market(bool isForecastingEnabled = true)
    {
        return new GetMarketResponse(
            MarketId,
            "OSRS",
            new Taxes(new FlatTax(1, 5000000, 0.02m)),
            isForecastingEnabled,
            "Old School RuneScape Grand Exchange.",
            null
        );
    }

    public static GetSymbolTradesResponse SelectedTrades()
    {
        return new GetSymbolTradesResponse(
            MinPrice: 9800000,
            MaxPrice: 10400000,
            AveragePrice: 10100000,
            TotalSpent: 404000000,
            Volume: 40,
            NumTransactions: 12,
            [
                new GetSymbolTradeBucketsResponse(
                    Price: 10000000,
                    Volume: 25,
                    TotalSpent: 250000000,
                    MinPrice: 9800000,
                    MaxPrice: 10200000,
                    NumTransactions: 7,
                    Date: GeneratedAt.AddHours(-2),
                    OpenPrice: 9900000,
                    ClosePrice: 10100000
                ),
                new GetSymbolTradeBucketsResponse(
                    Price: 10266666.6667m,
                    Volume: 15,
                    TotalSpent: 154000000,
                    MinPrice: 10100000,
                    MaxPrice: 10400000,
                    NumTransactions: 5,
                    Date: GeneratedAt.AddHours(-1),
                    OpenPrice: 10100000,
                    ClosePrice: 10300000
                ),
            ]
        );
    }

    public static GetMarketTradesResponse OneDayTrades()
    {
        return new GetMarketTradesResponse(
            SymbolId,
            "Armadyl godsword",
            null,
            TotalSpent: 404000000,
            MinPrice: 9800000,
            MaxPrice: 10400000,
            TotalVolume: 40,
            NumTransactions: 12,
            Limit: 8,
            Tax: 208000
        );
    }

    public static GetAllTradesResponse LatestTrade()
    {
        return new GetAllTradesResponse(
            new Id<Trade>("8c1f7f1e-0000-4000-8000-000000000003"),
            SymbolId,
            MarketId,
            "Armadyl godsword",
            "11802",
            10300000,
            2,
            GeneratedAt.AddMinutes(-5)
        );
    }

    public static GetSymbolSignalsResponse Signals()
    {
        return new GetSymbolSignalsResponse(
            SymbolId,
            "Armadyl godsword",
            [
                new SignalResponse(
                    "Rsi",
                    "RSI",
                    "Relative strength index.",
                    [InvestmentIntent.Buy, InvestmentIntent.Sell],
                    0.4m,
                    SignalDirection.Bullish,
                    SignalStrength.Medium
                ),
            ],
            new SignalSummary(0.4m, 1, 0, 0, IsFlipFavourable: true, IsMerchFavourable: false)
        );
    }

    public static GetSymbolSignalHistoryResponse SignalHistory()
    {
        return new GetSymbolSignalHistoryResponse(
            SymbolId,
            "Armadyl godsword",
            [
                new SignalHistoryResponse(
                    "Rsi",
                    "RSI",
                    "Relative strength index.",
                    [InvestmentIntent.Buy, InvestmentIntent.Sell],
                    0.4m,
                    SignalDirection.Bullish,
                    SignalStrength.Medium,
                    [
                        new SignalHistoryPoint(-0.3m, GeneratedAt.AddDays(-1).AddHours(-6)),
                        new SignalHistoryPoint(-0.2m, GeneratedAt.AddDays(-1).AddHours(-2)),
                        new SignalHistoryPoint(0.1m, GeneratedAt.AddHours(-2)),
                        new SignalHistoryPoint(0.4m, GeneratedAt.AddHours(-1)),
                    ]
                ),
                new SignalHistoryResponse(
                    "VolumeAnomaly",
                    "Volume Anomaly",
                    "Unusual volume.",
                    [InvestmentIntent.Buy],
                    0,
                    SignalDirection.Neutral,
                    SignalStrength.Weak,
                    []
                ),
            ],
            new SignalSummary(0.4m, 1, 0, 0, IsFlipFavourable: true, IsMerchFavourable: false)
        );
    }

    public static GetMarketForecastResponse Forecast()
    {
        var day = new GetMarketForecastPredictionResponse(
            AveragePrice: 10200000,
            MinPrice: 10000000,
            MaxPrice: 10500000,
            Volume: 38,
            Margin: 290000,
            Gain: 2320000,
            AveragePriceDelta: 0,
            MinPriceDelta: 0,
            MaxPriceDelta: 0,
            VolumeDelta: 0,
            GainDelta: 0
        );

        return new GetMarketForecastResponse(
            new Id<Forecast>("8c1f7f1e-0000-4000-8000-000000000004"),
            MarketId,
            SymbolId,
            "Armadyl godsword",
            null,
            new ForecastPoint(10100000, 9800000, 10400000, 40),
            day,
            day,
            day,
            day,
            day,
            day,
            day with
            {
                AveragePrice = 10600000,
            }
        );
    }

    public static IReadOnlyList<GetForecastEfficacyResponse> ForecastEfficacy()
    {
        return
        [
            new GetForecastEfficacyResponse(
                SymbolId,
                "Armadyl godsword",
                MarketId,
                "plutus-forecasting-v1",
                HorizonDays: 1,
                EvaluatedCount: 20,
                MeanAbsoluteError: 150000,
                MeanAbsolutePercentageError: 0.0145m,
                MeanBias: -20000,
                GeneratedAt.AddDays(-30),
                GeneratedAt.AddDays(-1)
            ),
        ];
    }

    public static IReadOnlyList<GetAllPositionsResponse> Positions()
    {
        return
        [
            new GetAllPositionsResponse(
                new Id<Position>("8c1f7f1e-0000-4000-8000-000000000005"),
                PositionSide.Buy,
                PositionStatus.Bought,
                MarketId,
                SymbolId,
                "Armadyl godsword",
                Cost: 9900000,
                Quantity: 2,
                TargetPrice: null,
                LinkedBuyPositionId: null,
                StrategyId: null,
                Notes: "Bought the dip",
                CreatedAt: GeneratedAt.AddDays(-2),
                UpdatedAt: GeneratedAt.AddDays(-2)
            ),
        ];
    }

    public static GetDailySymbolSummaryResponse Today()
    {
        return new GetDailySymbolSummaryResponse(
            AveragePrice: 10100000,
            MinPrice: 9800000,
            MaxPrice: 10400000,
            Volume: 40
        );
    }

    public static SymbolAnalystSnapshot Snapshot()
    {
        return new SymbolAnalystSnapshot(
            Symbol(),
            Market(),
            TimeFrame.OneDay,
            Today(),
            GeneratedAt
        );
    }

    public static GetSymbolTradesResponse FullTrades()
    {
        var buckets = Enumerable
            .Range(0, ChartPoints * 2)
            .Select(i => new GetSymbolTradeBucketsResponse(
                Price: 1234567.89m + i,
                Volume: 123456.78m,
                TotalSpent: 152415787654.32m,
                MinPrice: 1198765.43m + i,
                MaxPrice: 1287654.32m + i,
                NumTransactions: 98765,
                Date: GeneratedAt.AddDays(i - (ChartPoints * 2)),
                OpenPrice: 1212345.67m + i,
                ClosePrice: 1265432.1m + i
            ))
            .ToList();

        return new GetSymbolTradesResponse(
            1198765.43m,
            1287654.32m,
            1234567.89m,
            1975405239506.17m,
            1987654.32m,
            1580120,
            buckets
        );
    }

    public static SymbolAnalystSnapshot Full(TimeFrame timeFrame = TimeFrame.AllTime)
    {
        return Snapshot() with
        {
            Symbol = Symbol() with
            {
                Name = "Twisted ancestral colour kit (ornamented)",
                Subcode = "p2p",
                AdditionalFields = new AdditionalFields(
                    Limit: 70000,
                    HighAlch: 1234567,
                    LowAlch: 823045,
                    Exchange: "NASDAQ"
                ),
            },
            Market = Market(isForecastingEnabled: false) with
            {
                Description = new string('d', 400),
            },
            TimeFrame = timeFrame,
            Today = new GetDailySymbolSummaryResponse(
                1234567.89m,
                1198765.43m,
                1287654.32m,
                123456.78m
            ),
        };
    }
}
