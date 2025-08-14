using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Symbols.GetSymbolTrades;

public sealed class GetSymbolTradesHandler : QueryHandler<GetSymbolTradesInput, GetSymbolTradesResponse>
{
    private readonly IBucketTrades _bucketTrades;
    private readonly ILogger<GetSymbolTradesHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly IRepository<Trade> _tradeRepository;

    public GetSymbolTradesHandler(
        ILogger<GetSymbolTradesHandler> logger,
        IRepository<Trade> tradeRepository,
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

        var trades = await _queryExecutor.ToList(
            _bucketTrades.GetBucketedTradesQuery(
                _tradeRepository.AsQueryable(cancellationToken).Where(t =>
                    t.SymbolId == query.SymbolId && (since == null || t.CreatedAt >= since)
                ),
                query.NumBuckets,
                cancellationToken
            ),
            cancellationToken
        );

        var symbolDetail = trades
            .GroupBy(x => x.SymbolId)
            .Select(g => new
                {
                    g.Key,
                    MinPrice = g.Min(x => x.MinPrice),
                    MaxPrice = g.Max(x => x.MaxPrice),
                    TotalSpent = g.Sum(x => x.TotalSpent),
                    Volume = g.Sum(x => x.Volume),
                    NumTransactions = g.Sum(x => x.NumTransactions),
                    Trades = g.ToList()
                }
            )
            .Select(x => new GetSymbolTradesResponse(
                    x.MinPrice,
                    x.MaxPrice,
                    x.TotalSpent / x.Volume,
                    x.TotalSpent,
                    x.Volume,
                    x.NumTransactions,
                    x.Trades
                        .OrderBy(x => x.Date)
                        .Select(t => new GetSymbolTradeBucketsResponse(
                                t.Price,
                                t.Volume,
                                t.TotalSpent,
                                t.MinPrice,
                                t.MaxPrice,
                                t.NumTransactions,
                                t.Date
                            )
                        )
                )
            )
            .FirstOrDefault();

        var response = symbolDetail ?? new GetSymbolTradesResponse(
            0m,
            0m,
            0m,
            0m,
            0m,
            0,
            []
        );

        _logger.LogDebug("Successfully handled get symbol trades request.");
        return response;
    }
}