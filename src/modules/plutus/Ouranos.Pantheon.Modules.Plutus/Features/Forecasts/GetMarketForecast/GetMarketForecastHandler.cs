using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database.Querying;
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
    private static readonly FilterBuilder<GetMarketForecastResponse> FilterBuilder =
        new FilterBuilder<GetMarketForecastResponse>()
            .On(nameof(GetMarketForecastResponse.SymbolId), x => x.SymbolId)
            .On(
                nameof(GetMarketForecastResponse.SymbolName),
                x => x.SymbolName,
                caseInsensitive: true
            )
            .On(
                nameof(GetMarketForecastResponse.SymbolSubcode),
                x => x.SymbolSubcode,
                caseInsensitive: true
            );

    private static readonly SortBuilder<GetMarketForecastResponse> SortBuilder =
        new SortBuilder<GetMarketForecastResponse>()
            .On(nameof(GetMarketForecastResponse.SymbolName), x => x.SymbolName)
            .On(
                $"{nameof(GetMarketForecastResponse.DayOne)}.{nameof(GetMarketForecastPredictionResponse.Gain)}",
                x => x.DayOne.Gain
            )
            .On(
                $"{nameof(GetMarketForecastResponse.DayOne)}.{nameof(GetMarketForecastPredictionResponse.Margin)}",
                x => x.DayOne.Margin
            )
            .On(
                $"{nameof(GetMarketForecastResponse.DayTwo)}.{nameof(GetMarketForecastPredictionResponse.Gain)}",
                x => x.DayTwo.Gain
            )
            .Default(x => x.DayOne.Gain);

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

        var flatTax = market.Taxes.Flat ?? new FlatTax(0, 0, 0);

        var forecasts = await _dbContext
            .Forecasts.AsNoTracking()
            .Where(f => f.MarketId == input.MarketId && f.Predictions.Count >= 7)
            .WhereLatestPerSymbol()
            .Select(f => new
            {
                f.Id,
                f.MarketId,
                f.SymbolId,
                SymbolName = f.Symbol.Name,
                SymbolSubcode = f.Symbol.Subcode,
                f.Latest,
                Predictions = f.Predictions.Select(p => new
                {
                    p.AveragePrice,
                    p.MaxPrice,
                    p.MinPrice,
                    p.Volume,
                    Margin = p.AveragePrice
                        - (p.AveragePrice * flatTax.Rate > 0 ? 0 : p.AveragePrice * flatTax.Rate)
                        - f.Latest.AveragePrice,
                }),
            })
            .Select(f => new
            {
                f.Id,
                f.MarketId,
                f.SymbolId,
                f.SymbolName,
                f.SymbolSubcode,
                f.Latest,
                Predictions = f.Predictions.Select(p => new GetMarketForecastPredictionResponse(
                    p.AveragePrice,
                    p.MinPrice,
                    p.MaxPrice,
                    p.Volume,
                    p.Margin,
                    p.Margin * p.Volume,
                    p.AveragePrice - f.Latest.AveragePrice,
                    p.MinPrice - f.Latest.MinPrice,
                    p.MaxPrice - f.Latest.MaxPrice,
                    p.Volume - f.Latest.Volume,
                    p.AveragePrice * p.Volume - f.Latest.AveragePrice * f.Latest.Volume
                )),
            })
            .ToListAsync(cancellationToken);

        var responses = forecasts
            .Select(f => new GetMarketForecastResponse(
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

        var filtered = responses.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var page = filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get market forecast query.");
        return new PagedResponse<GetMarketForecastResponse>(
            page,
            totalCount,
            input.Skip,
            input.Take
        );
    }
}
