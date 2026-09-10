using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database.Querying;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;

public sealed class GetMarketForecastHandler
    : IPantheonHandler<GetMarketForecastInput, PagedResponse<GetMarketForecastResponse>>
{
    private static readonly FilterBuilder<Forecast> FilterBuilder = new FilterBuilder<Forecast>()
        .On(nameof(GetMarketForecastResponse.SymbolId), f => f.SymbolId)
        .On(nameof(GetMarketForecastResponse.SymbolName), f => f.Symbol.Name, caseInsensitive: true)
        .On(
            nameof(GetMarketForecastResponse.SymbolSubcode),
            f => f.Symbol.Subcode,
            caseInsensitive: true
        );

    private const string DayOneGainField =
        $"{nameof(GetMarketForecastResponse.DayOne)}.{nameof(GetMarketForecastPredictionResponse.Gain)}";

    private const string DayOneMarginField =
        $"{nameof(GetMarketForecastResponse.DayOne)}.{nameof(GetMarketForecastPredictionResponse.Margin)}";

    private const string DayTwoGainField =
        $"{nameof(GetMarketForecastResponse.DayTwo)}.{nameof(GetMarketForecastPredictionResponse.Gain)}";

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetMarketForecastHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetMarketForecastHandler(
        ILogger<GetMarketForecastHandler> logger,
        PlutusDbContext dbContext,
        IOptions<QueryOptions> queryOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(queryOptions);

        _logger = logger;
        _dbContext = dbContext;
        _queryOptions = queryOptions;
    }

    public async Task<PagedResponse<GetMarketForecastResponse>> Handle(
        GetMarketForecastInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get market forecast query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var market = await _dbContext
            .Markets.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == input.MarketId, cancellationToken);

        Guard.Against.NotFound(input.MarketId, market);

        var taxRate = (market.Taxes.Flat ?? new FlatTax(0, 0, 0)).Rate;

        var forecasts = _dbContext
            .Forecasts.AsNoTracking()
            .Where(f => f.MarketId == input.MarketId && f.Predictions.Count >= 7)
            .WhereLatestPerSymbol()
            .FilterBy(input.Filter, FilterBuilder);

        var totalCount = await forecasts.CountAsync(cancellationToken);

        var page = await forecasts
            .Select(f => new
            {
                f.Id,
                f.MarketId,
                f.SymbolId,
                SymbolName = f.Symbol.Name,
                SymbolSubcode = f.Symbol.Subcode,
                f.Latest,
                DayOneMargin = f.Predictions.First().AveragePrice
                    - Math.Min(0, f.Predictions.First().AveragePrice * taxRate)
                    - f.Latest.AveragePrice,
                DayOneGain = (
                    f.Predictions.First().AveragePrice
                    - Math.Min(0, f.Predictions.First().AveragePrice * taxRate)
                    - f.Latest.AveragePrice
                ) * f.Predictions.First().Volume,
                DayTwoGain = (
                    f.Predictions.Skip(1).First().AveragePrice
                    - Math.Min(0, f.Predictions.Skip(1).First().AveragePrice * taxRate)
                    - f.Latest.AveragePrice
                ) * f.Predictions.Skip(1).First().Volume,
                Predictions = f
                    .Predictions.Select(p => new GetMarketForecastPredictionResponse(
                        p.AveragePrice,
                        p.MinPrice,
                        p.MaxPrice,
                        p.Volume,
                        p.AveragePrice
                            - Math.Min(0, p.AveragePrice * taxRate)
                            - f.Latest.AveragePrice,
                        (
                            p.AveragePrice
                            - Math.Min(0, p.AveragePrice * taxRate)
                            - f.Latest.AveragePrice
                        ) * p.Volume,
                        p.AveragePrice - f.Latest.AveragePrice,
                        p.MinPrice - f.Latest.MinPrice,
                        p.MaxPrice - f.Latest.MaxPrice,
                        p.Volume - f.Latest.Volume,
                        p.AveragePrice * p.Volume - f.Latest.AveragePrice * f.Latest.Volume
                    ))
                    .ToList(),
            })
            .SortBy(
                input.SortField,
                input.SortDirection,
                builder =>
                    builder
                        .On(nameof(GetMarketForecastResponse.SymbolName), x => x.SymbolName)
                        .On(DayOneMarginField, x => x.DayOneMargin)
                        .On(DayOneGainField, x => x.DayOneGain)
                        .On(DayTwoGainField, x => x.DayTwoGain)
                        .Default(x => x.DayOneGain)
            )
            .Paginate(input.Skip, input.Take)
            .ToListAsync(cancellationToken);

        var items = page.Select(f => new GetMarketForecastResponse(
                f.Id,
                f.MarketId,
                f.SymbolId,
                f.SymbolName,
                f.SymbolSubcode,
                f.Latest,
                f.Predictions.ElementAt(0),
                f.Predictions.ElementAt(1),
                f.Predictions.ElementAt(2),
                f.Predictions.ElementAt(3),
                f.Predictions.ElementAt(4),
                f.Predictions.ElementAt(5),
                f.Predictions.ElementAt(6)
            ))
            .ToList();

        _logger.LogDebug("Successfully handled get market forecast query.");
        return new PagedResponse<GetMarketForecastResponse>(
            items,
            totalCount,
            input.Skip,
            input.Take
        );
    }
}
