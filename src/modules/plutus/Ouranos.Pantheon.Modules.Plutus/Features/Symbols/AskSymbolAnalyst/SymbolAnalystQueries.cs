using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.AskSymbolAnalyst;

/// <summary>
/// Queries that more than one assistant tool runs.
/// </summary>
internal static class SymbolAnalystQueries
{
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

    public static async Task<(GetSymbolResponse Symbol, GetMarketResponse Market)> LoadSymbolAsync(
        IMessageBus bus,
        Id<Symbol> symbolId,
        CancellationToken cancellationToken
    )
    {
        var symbol = await bus.InvokeAsync<GetSymbolResponse>(
            new GetSymbolInput(symbolId),
            cancellationToken
        );

        var market = await bus.InvokeAsync<GetMarketResponse>(
            new GetMarketInput(symbol.MarketId),
            cancellationToken
        );

        return (symbol, market);
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
            trades.MinPrice > 0 ? SymbolAnalystData.Percent(trades.Roi) : null
        );
    }
}
