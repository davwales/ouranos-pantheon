using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Models.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Options;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetHistoricalData;

public sealed class GetHistoricalDataHandler
    : QueryHandler<GetHistoricalDataInput, WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>>
{
    private readonly IBucketHistoricalData _bucketHistoricalData;
    private readonly ForecastingOptions _forecastingOptions;
    private readonly ILogger<GetHistoricalDataHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly IRepository<Trade> _tradeRepository;

    public GetHistoricalDataHandler(
        ILogger<GetHistoricalDataHandler> logger,
        IBucketHistoricalData bucketHistoricalData,
        IRepository<Trade> tradeRepository,
        IQueryExecutor queryExecutor,
        IOptions<ForecastingOptions> forecastingOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(bucketHistoricalData);
        Guard.Against.Null(tradeRepository);
        Guard.Against.Null(queryExecutor);
        Guard.Against.Null(forecastingOptions?.Value);

        _logger = logger;
        _bucketHistoricalData = bucketHistoricalData;
        _tradeRepository = tradeRepository;
        _queryExecutor = queryExecutor;
        _forecastingOptions = forecastingOptions.Value;
    }

    public override async Task<WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>> Handle(
        GetHistoricalDataInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get historical data query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var end = DateTime.UtcNow.Date;
        var start = end.AddDays(-_forecastingOptions.SequenceLength);

        var tradeFilter = _tradeRepository.AsQueryable(cancellationToken)
            .Where(trade =>
                trade.CreatedAt >= start &&
                trade.CreatedAt < end &&
                query.SymbolIds.Contains(trade.Metadata.SymbolId)
            );

        var dataQuery = _bucketHistoricalData.ApplyBucketing(tradeFilter)
            .OrderBy(x => x.Id.Bucket)
            .GroupBy(x => x.Id.SymbolId)
            .Select(g => new HistoricalDataDto(
                g.Key,
                g.Select(x => new ForecastPoint(
                    x.TotalSpent / x.Volume,
                    x.MinPrice,
                    x.MaxPrice,
                    x.Volume
                )).ToList()
            ))
            .Where(x => x.HistoricalSymbolData.Count == _forecastingOptions.SequenceLength);

        var historicalData = await _queryExecutor.ToList(dataQuery, cancellationToken);

        var response = new WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>(
            historicalData.ToDictionary(x => x.Id, x => x.HistoricalSymbolData)
        );

        _logger.LogDebug("Successfully handled get historical data query.");
        return response;
    }
}