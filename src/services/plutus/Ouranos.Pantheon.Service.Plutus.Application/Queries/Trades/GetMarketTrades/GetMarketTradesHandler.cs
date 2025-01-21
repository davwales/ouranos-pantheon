using Ardalis.GuardClauses;
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
        Guard.Against.Null(logger);
        Guard.Against.Null(marketRepository);
        Guard.Against.Null(tradeRepository);

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
                Limit = g.First().Metadata.AdditionalFields.Limit ?? g.Sum(t => t.Volume)
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
                Tax = x.MaxPrice >= 100m ? x.MaxPrice * 0.01m : 0m
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
                x.MaxPrice - x.MinPrice - x.Tax, // margin
                x.TotalSpent / x.TotalVolume, // average price
                (x.MaxPrice - x.MinPrice - x.Tax) / x.MinPrice * 100, // roi
                (x.MaxPrice - x.MinPrice - x.Tax) * (x.TotalVolume > x.Limit ? x.Limit : x.TotalVolume), // total gain
                x.Limit
            ));

        var response = new WrapperResponse<IQueryable<GetMarketTradesResponse>>(tradeQuery);

        _logger.LogDebug("Successfully handled symbol statistics request.");
        return await Task.FromResult(response);
    }
}