using Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.AskMarketAnalyst;

internal static class MarketAnalystSnapshotFactory
{
    public static readonly Id<Market> MarketId = new("5a2e0c11-0000-4000-8000-000000000001");
    public static readonly Id<Symbol> TwistedBowId = new("5a2e0c11-0000-4000-8000-000000000002");
    public static readonly Id<Symbol> RunePlatebodyId = new("5a2e0c11-0000-4000-8000-000000000003");
    public static readonly DateTimeOffset GeneratedAt = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);

    public static GetMarketResponse Market(bool isTaxed = true)
    {
        return new GetMarketResponse(
            MarketId,
            "OSRS",
            new Taxes(isTaxed ? new FlatTax(1, 5000000, 0.02m) : null),
            IsForecastingEnabled: true,
            "Old School RuneScape Grand Exchange.",
            null
        );
    }

    public static MarketAnalystContext Context()
    {
        return new MarketAnalystContext(
            MarketId,
            TimeFrame.OneDay,
            ["TotalVolume:gte:2"],
            "Roi",
            "desc",
            Skip: 10,
            Take: 2
        );
    }

    public static GetMarketOverviewResponse Overview()
    {
        return new GetMarketOverviewResponse(
            AveragePrice: 1234.5m,
            TotalSpent: 3086250000,
            Volume: 2500000,
            NumTransactions: 48000,
            [
                new GetMarketOverviewBucketResponse(1100, 0, 0, 0, GeneratedAt.AddHours(-2)),
                new GetMarketOverviewBucketResponse(1000, 100, 100000, 4, GeneratedAt.AddHours(-4)),
                new GetMarketOverviewBucketResponse(1300, 0, 0, 0, GeneratedAt.AddHours(-1)),
                new GetMarketOverviewBucketResponse(1200, 300, 360000, 9, GeneratedAt.AddHours(-3)),
            ]
        );
    }

    public static GetVolumeHeatmapResponse Heatmap()
    {
        return new GetVolumeHeatmapResponse([
            new HeatmapCellResponse(DayOfWeek: 0, Hour: 18, TotalTrades: 500, Percentage: 25),
            new HeatmapCellResponse(DayOfWeek: 5, Hour: 20, TotalTrades: 900, Percentage: 45),
            new HeatmapCellResponse(DayOfWeek: 6, Hour: 3, TotalTrades: 10, Percentage: 0.5m),
            new HeatmapCellResponse(DayOfWeek: 2, Hour: 4, TotalTrades: 0, Percentage: 0),
            new HeatmapCellResponse(DayOfWeek: 3, Hour: 12, TotalTrades: 590, Percentage: 29.5m),
        ]);
    }

    public static GetMarketTradesResponse TwistedBow()
    {
        return new GetMarketTradesResponse(
            TwistedBowId,
            "Twisted bow",
            null,
            TotalSpent: 2930000000,
            MinPrice: 1420000000,
            MaxPrice: 1510000000,
            TotalVolume: 2,
            NumTransactions: 2,
            Limit: 8,
            Tax: 5000000
        );
    }

    public static GetMarketTradesResponse RunePlatebody()
    {
        return new GetMarketTradesResponse(
            RunePlatebodyId,
            "Rune platebody",
            null,
            TotalSpent: 462000000,
            MinPrice: 38000,
            MaxPrice: 39500,
            TotalVolume: 12000,
            NumTransactions: 1500,
            Limit: 125,
            Tax: 790
        );
    }

    public static PagedResponse<GetMarketTradesResponse> View(params GetMarketTradesResponse[] rows)
    {
        return new PagedResponse<GetMarketTradesResponse>(rows, 42, 10, rows.Length);
    }

    public static MarketAnalystSnapshot Snapshot(
        GetMarketResponse? market = null,
        MarketAnalystContext? context = null,
        GetMarketOverviewResponse? overview = null,
        GetVolumeHeatmapResponse? heatmap = null,
        PagedResponse<GetMarketTradesResponse>? view = null
    )
    {
        return new MarketAnalystSnapshot(
            market ?? Market(),
            context ?? Context(),
            overview ?? Overview(),
            heatmap ?? Heatmap(),
            view ?? View(TwistedBow(), RunePlatebody()),
            TrendPoints: 2,
            GeneratedAt
        );
    }
}
