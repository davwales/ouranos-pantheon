using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Trades.GetMarketTrades;

public sealed class GetMarketTradesHandler :
    QueryHandler<GetMarketTradesInput, WrapperResponse<IQueryable<GetMarketTradesResponse>>>
{
    private readonly ILogger<GetMarketTradesHandler> _logger;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public GetMarketTradesHandler(
        ILogger<GetMarketTradesHandler> logger,
        ICrudRepository<Market> marketRepository,
        ICrudRepository<Trade> tradeRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(marketRepository);
        ArgumentNullException.ThrowIfNull(tradeRepository);

        _logger = logger;
        _tradeRepository = tradeRepository;
    }

    protected override async Task<WrapperResponse<IQueryable<GetMarketTradesResponse>>> Handle(
        GetMarketTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle symbol statistics query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset? since = query.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(query.Seconds.Value)
            : null;

        var tradeQuery = _tradeRepository.AsQueryable(cancellationToken)
            .Where(x => x.Metadata.MarketId == query.MarketId &&
                        (since == null || x.CreatedAt >= since))
            .GroupBy(t => t.Metadata.SymbolId)
            .Select(g => new
            {
                g.First().Metadata.SymbolId,
                g.First().Metadata.SymbolName,
                g.First().Metadata.SymbolCode,
                g.First().Metadata.SymbolSubcode,
                TotalSpent = g.Sum(t => t.Price * t.Volume),
                MinPrice = g.Min(t => t.Price),
                MaxPrice = g.Max(t => t.Price),
                TotalVolume = g.Sum(t => t.Volume),
                NumTransactions = g.Count(),
                g.First().Metadata.AdditionalFields.Limit
            })
            .Select(x => new
            {
                x.SymbolId,
                x.SymbolName,
                x.SymbolCode,
                x.SymbolSubcode,
                x.TotalSpent,
                x.MinPrice,
                x.MaxPrice,
                x.TotalVolume,
                x.NumTransactions,
                Limit = x.Limit ?? x.TotalVolume,
                FlatTax = x.MaxPrice >= 100m ? x.MaxPrice * 0.01m : 0m
            })
            .Select(x => new
            {
                x.SymbolId,
                x.SymbolName,
                x.SymbolCode,
                x.SymbolSubcode,
                x.TotalSpent,
                x.MinPrice,
                x.MaxPrice,
                x.TotalVolume,
                x.NumTransactions,
                x.Limit,
                EffectiveLimit = x.TotalVolume > x.Limit ? x.Limit : x.TotalVolume,
                x.FlatTax,
                Tax = x.FlatTax,
                Margin = x.MaxPrice - x.MinPrice - x.FlatTax,
                AveragePrice = x.TotalSpent / x.TotalVolume
            })
            .Select(x => new
            {
                x.SymbolId,
                x.SymbolName,
                x.SymbolCode,
                x.SymbolSubcode,
                x.TotalSpent,
                x.MinPrice,
                x.MaxPrice,
                x.TotalVolume,
                x.NumTransactions,
                x.Limit,
                x.FlatTax,
                x.Tax,
                x.Margin,
                x.AveragePrice,
                TotalGain = x.Margin * x.EffectiveLimit,
                Roi = x.Margin / x.MinPrice * 100
            })
            .Select(x => new GetMarketTradesResponse(
                x.SymbolId,
                x.SymbolName,
                x.SymbolCode,
                x.SymbolSubcode,
                x.TotalSpent,
                x.MinPrice,
                x.MaxPrice,
                x.TotalVolume,
                x.NumTransactions,
                x.Margin,
                x.AveragePrice,
                x.Roi,
                x.TotalGain,
                x.Limit)
            );

        var response = new WrapperResponse<IQueryable<GetMarketTradesResponse>>(tradeQuery);

        _logger.LogDebug("Successfully handled symbol statistics request.");
        return await Task.FromResult(response);
    }
}