using MediatR;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Trades.GetMarketTrades;

public sealed class GetMarketTradesHandler : IRequestHandler<GetMarketTradesInput, IQueryable<GetMarketTradesResponse>>
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

    public async Task<IQueryable<GetMarketTradesResponse>> Handle(
        GetMarketTradesInput request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogTrace("Attempting to handle symbol statistics request '{@request}'.", request);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset? since = request.Seconds.HasValue
            ? DateTimeOffset.UtcNow - TimeSpan.FromSeconds(request.Seconds.Value)
            : null;

        var query = _tradeRepository.AsQueryable(cancellationToken)
            .Where(x => x.Metadata.MarketId == request.MarketId &&
                        (since == null || x.CreatedAt >= since))
            .GroupBy(t => t.Metadata.SymbolId)
            .Select(g => new
            {
                g.First().Metadata.SymbolId,
                g.First().Metadata.SymbolName,
                g.First().Metadata.SymbolCode,
                g.First().Metadata.SymbolSubCode,
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
                x.SymbolSubCode,
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
                x.SymbolSubCode,
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
                x.SymbolSubCode,
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
                x.SymbolSubCode,
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

        _logger.LogDebug("Successfully handled symbol statistics request.");
        return await Task.FromResult(query);
    }
}