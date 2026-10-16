using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools;

/// <summary>
/// Queries that more than one assistant tool runs.
/// </summary>
internal static class AssistantToolQueries
{
    internal const string ForecastingDisabled = "Forecasting is disabled for this market.";

    internal static readonly TimeFrame[] SummaryTimeFrames =
    [
        TimeFrame.OneDay,
        TimeFrame.OneWeek,
        TimeFrame.OneMonth,
    ];

    public static string[] SymbolFilter(Id<Symbol> symbolId)
    {
        return [$"SymbolId:eq:{symbolId}"];
    }

    public static async Task<bool> IsForecastingEnabledAsync(
        IMessageBus bus,
        Id<Market> marketId,
        CancellationToken cancellationToken
    )
    {
        var market = await bus.InvokeAsync<GetMarketResponse>(
            new GetMarketInput(marketId),
            cancellationToken
        );

        return market.IsForecastingEnabled;
    }

    public static async Task<GetMarketForecastResponse?> LoadForecastAsync(
        IMessageBus bus,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken
    )
    {
        var forecasts = await bus.InvokeAsync<PagedResponse<GetMarketForecastResponse>>(
            new GetMarketForecastInput(marketId, Take: 1, Filter: SymbolFilter(symbolId)),
            cancellationToken
        );

        return forecasts.Items.FirstOrDefault();
    }

    public static Task<PriceSummaryData[]> LoadPriceSummariesAsync(
        IMessageBus bus,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken
    )
    {
        return Task.WhenAll(
            SummaryTimeFrames.Select(timeFrame =>
                LoadPriceSummaryAsync(bus, marketId, timeFrame, symbolId, cancellationToken)
            )
        );
    }

    public static async Task<PriceSummaryData> LoadPriceSummaryAsync(
        IMessageBus bus,
        Id<Market> marketId,
        TimeFrame timeFrame,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken
    )
    {
        var page = await bus.InvokeAsync<PagedResponse<GetMarketTradesResponse>>(
            new GetMarketTradesInput(marketId, timeFrame, Take: 1, Filter: SymbolFilter(symbolId)),
            cancellationToken
        );

        if (page.Items.FirstOrDefault() is not { TotalVolume: > 0 } trades)
        {
            return new PriceSummaryData(timeFrame, HasTrades: false);
        }

        return new PriceSummaryData(
            timeFrame,
            HasTrades: true,
            trades.AveragePrice,
            trades.MinPrice,
            trades.MaxPrice,
            trades.TotalVolume,
            trades.NumTransactions,
            trades.Margin,
            trades.MinPrice > 0 ? AssistantToolData.Percent(trades.Roi) : null
        );
    }
}
