using MediatR;
using Microsoft.Extensions.Logging;
using Talos.Olympus.Core.Application.Interfaces.Common;
using Talos.Olympus.Service.Plutus.Application.Interfaces.Trades;
using Talos.Olympus.Service.Plutus.Domain.Markets;
using Talos.Olympus.Service.Plutus.Domain.Trades;

namespace Talos.Olympus.Service.Plutus.Application.Queries.Trades.GetSymbolTrades;

public sealed class GetSymbolTradesHandler : IRequestHandler<GetSymbolTradesInput, GetSymbolTradesResponse>
{
    private readonly IBucketTrades _bucketTrades;
    private readonly ILogger<GetSymbolTradesHandler> _logger;
    private readonly ICrudRepository<Market> _marketRepository;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICrudRepository<Trade> _tradeRepository;

    public GetSymbolTradesHandler(
        ILogger<GetSymbolTradesHandler> logger,
        ICrudRepository<Market> marketRepository,
        ICrudRepository<Trade> tradeRepository,
        IBucketTrades bucketTrades,
        IQueryExecutor queryExecutor
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(marketRepository);
        ArgumentNullException.ThrowIfNull(tradeRepository);
        ArgumentNullException.ThrowIfNull(bucketTrades);
        ArgumentNullException.ThrowIfNull(queryExecutor);

        _logger = logger;
        _marketRepository = marketRepository;
        _tradeRepository = tradeRepository;
        _bucketTrades = bucketTrades;
        _queryExecutor = queryExecutor;
    }

    public async Task<GetSymbolTradesResponse> Handle(
        GetSymbolTradesInput request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbol trades request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        DateTime? since = request.Seconds.HasValue
            ? DateTime.UtcNow - TimeSpan.FromSeconds(request.Seconds.Value)
            : null;

        var market = await _marketRepository.Read(request.MarketId, cancellationToken);

        var tradesQuery = _tradeRepository.AsQueryable(cancellationToken)
            .Where(t => t.Metadata.Symbol.Id == request.SymbolId && (since == null || t.CreatedAt >= since));

        var symbolTradesQuery = _bucketTrades.GetBucketedTradesQuery(tradesQuery, request.NumBuckets, cancellationToken)
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

        var symbolTrades = await _queryExecutor.FirstOrDefaultAsync(symbolTradesQuery, cancellationToken);

        _logger.LogDebug("Successfully handled get symbol trades request.");
        return symbolTrades ?? new GetSymbolTradesResponse(0m, 0m, 0m, 0m, 0m, 0m, 0m, 0, 0m, []);
    }
}