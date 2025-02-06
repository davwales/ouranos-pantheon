using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Trades.GetSymbolTrades;

public sealed class GetSymbolTradesHandler :
    QueryHandler<GetSymbolTradesInput, GetSymbolTradesResponse>
{
    private readonly IBucketTrades _bucketTrades;
    private readonly ILogger<GetSymbolTradesHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public GetSymbolTradesHandler(
        ILogger<GetSymbolTradesHandler> logger,
        ICrudRepository<Trade> tradeRepository,
        IBucketTrades bucketTrades,
        IQueryExecutor queryExecutor
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(tradeRepository);
        Guard.Against.Null(bucketTrades);
        Guard.Against.Null(queryExecutor);

        _logger = logger;
        _tradeRepository = tradeRepository;
        _bucketTrades = bucketTrades;
        _queryExecutor = queryExecutor;
    }

    public override async Task<GetSymbolTradesResponse> Handle(
        GetSymbolTradesInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbol trades query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset? since = query.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(query.Seconds.Value)
            : null;

        var tradesQuery = _tradeRepository.AsQueryable(cancellationToken)
            .Where(t => t.Metadata.SymbolId == query.SymbolId && (since == null || t.CreatedAt >= since));

        var symbolTradesQuery = _bucketTrades
            .GetBucketedTradesQuery(tradesQuery, query.NumBuckets, cancellationToken)
            .Select(x => new
            {
                x.SymbolId,
                x.Date,
                x.TotalSpent,
                x.Volume,
                x.MinPrice,
                x.MaxPrice,
                x.NumTransactions,
                Price = x.TotalSpent / x.Volume,
                Margin = x.MaxPrice - x.MinPrice
            })
            .GroupBy(x => x.SymbolId)
            .Select(g => new
            {
                g.Last().SymbolId,
                TotalPrice = g.Sum(x => x.Price),
                MinPrice = g.Min(x => x.MinPrice),
                MaxPrice = g.Max(x => x.MaxPrice),
                TotalSpent = g.Sum(x => x.TotalSpent),
                TotalVolume = g.Sum(x => x.Volume),
                NumTransactions = g.Sum(x => x.NumTransactions),
                Trades = g.ToList()
            })
            // Taxes
            .Select(x => new
            {
                x.SymbolId,
                x.TotalPrice,
                x.MinPrice,
                x.MaxPrice,
                x.TotalSpent,
                x.TotalVolume,
                x.NumTransactions,
                x.Trades,
                Margin = x.MaxPrice - x.MinPrice, // add  tax
                AveragePrice = x.TotalPrice / x.TotalVolume
            })
            .Select(x => new GetSymbolTradesResponse(
                x.MinPrice,
                x.MaxPrice,
                x.AveragePrice,
                x.TotalSpent,
                x.Margin,
                x.Margin * x.TotalVolume,
                x.AveragePrice * x.TotalVolume,
                x.NumTransactions,
                0m, //tax
                x.Trades.Select(t => new GetSymbolTradeBucketsResponse(
                    t.Price,
                    t.Volume,
                    t.TotalSpent,
                    t.MinPrice,
                    t.MaxPrice,
                    t.Margin,
                    t.NumTransactions,
                    t.Date
                )).ToList()
            ));

        var symbolTrades = await _queryExecutor.FirstOrDefaultAsync<GetSymbolTradesResponse?>(
            symbolTradesQuery,
            cancellationToken
        );

        var response = symbolTrades ?? new GetSymbolTradesResponse(
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            0,
            0m,
            []
        );

        _logger.LogDebug("Successfully handled get symbol trades request.");
        return response;
    }
}