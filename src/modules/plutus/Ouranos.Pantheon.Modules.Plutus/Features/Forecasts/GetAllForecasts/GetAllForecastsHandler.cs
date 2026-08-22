using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database.Querying;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common.Sorting;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;

public sealed class GetAllForecastsHandler
    : IPantheonHandler<GetAllForecastsInput, PagedResponse<GetAllForecastsResponse>>
{
    private static readonly FilterBuilder<Forecast> FilterBuilder = new FilterBuilder<Forecast>()
        .On(nameof(Forecast.SymbolId), f => f.SymbolId)
        .On(nameof(Forecast.MarketId), f => f.MarketId);

    private static readonly SortBuilder<Forecast> SortBuilder = new SortBuilder<Forecast>()
        .On(nameof(GetAllForecastsResponse.SymbolName), f => f.Symbol.Name)
        .Default(f => f.SymbolId);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllForecastsHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllForecastsHandler(
        ILogger<GetAllForecastsHandler> logger,
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

    public async Task<PagedResponse<GetAllForecastsResponse>> Handle(
        GetAllForecastsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all forecasts query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var q = _dbContext
            .Forecasts.AsQueryable()
            .AsNoTracking()
            .WhereLatestPerSymbol()
            .FilterBy(input.Filter, FilterBuilder);

        var totalCount = await q.CountAsync(cancellationToken);

        var items = await q.SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .Select(f => new GetAllForecastsResponse(
                f.Id,
                f.MarketId,
                f.SymbolId,
                f.Symbol.Name,
                f.Latest
            ))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get all forecasts request.");
        return new PagedResponse<GetAllForecastsResponse>(
            items,
            totalCount,
            input.Skip,
            input.Take
        );
    }
}
