using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Models.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Options;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetHistoricalData;

public sealed class GetHistoricalDataHandler
    : QueryHandler<GetHistoricalDataInput, WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>>
{
    private readonly IBucketHistoricalData _bucketHistoricalData;
    private readonly IOptions<ForecastingOptions> _forecastingOptions;
    private readonly ILogger<GetHistoricalDataHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public GetHistoricalDataHandler(
        ILogger<GetHistoricalDataHandler> logger,
        IBucketHistoricalData bucketHistoricalData,
        IPlutusUnitOfWork unitOfWork,
        IQueryExecutor queryExecutor,
        IOptions<ForecastingOptions> forecastingOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(bucketHistoricalData);
        Guard.Against.Null(unitOfWork);
        Guard.Against.Null(queryExecutor);
        Guard.Against.Null(forecastingOptions);

        _logger = logger;
        _bucketHistoricalData = bucketHistoricalData;
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
        _forecastingOptions = forecastingOptions;
    }

    public override async Task<WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>> Handle(
        GetHistoricalDataInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get historical data query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var end = DateTime.UtcNow.Date;
        var start = end.AddDays(-_forecastingOptions.Value.SequenceLength);

        var tradeFilter = _unitOfWork.Trades.AsQueryable(cancellationToken)
            .Where(trade =>
                trade.CreatedAt >= start &&
                trade.CreatedAt < end &&
                query.SymbolIds.Contains(trade.SymbolId)
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
                        )
                    ).ToList()
                )
            )
            .Where(x => x.HistoricalSymbolData.Count == _forecastingOptions.Value.SequenceLength);

        var historicalData = await _queryExecutor.ToList(dataQuery, cancellationToken);

        var response = new WrapperResponse<Dictionary<Id<Symbol>, List<ForecastPoint>>>(
            historicalData.ToDictionary(x => x.Id, x => x.HistoricalSymbolData)
        );

        _logger.LogDebug("Successfully handled get historical data query.");
        return response;
    }
}