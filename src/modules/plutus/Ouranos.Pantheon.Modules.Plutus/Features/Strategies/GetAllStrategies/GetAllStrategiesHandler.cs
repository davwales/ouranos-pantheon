using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Filtering;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Pagination;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies;

public sealed class GetAllStrategiesHandler
    : IPantheonHandler<GetAllStrategiesInput, PagedResponse<GetAllStrategiesResponse>>
{
    private static readonly FilterBuilder<GetAllStrategiesResponse> FilterBuilder =
        new FilterBuilder<GetAllStrategiesResponse>()
            .On(nameof(GetAllStrategiesResponse.Name), x => x.Name, caseInsensitive: true)
            .On(nameof(GetAllStrategiesResponse.IsActive), x => x.IsActive)
            .On(nameof(GetAllStrategiesResponse.BacktestCount), x => x.BacktestCount)
            .On(nameof(GetAllStrategiesResponse.LastBacktestReturn), x => x.LastBacktestReturn)
            .On(nameof(GetAllStrategiesResponse.LastBacktestWinRate), x => x.LastBacktestWinRate);

    private static readonly SortBuilder<GetAllStrategiesResponse> SortBuilder =
        new SortBuilder<GetAllStrategiesResponse>()
            .On(nameof(GetAllStrategiesResponse.Name), x => x.Name)
            .On(nameof(GetAllStrategiesResponse.BacktestCount), x => x.BacktestCount)
            .On(nameof(GetAllStrategiesResponse.LastBacktestReturn), x => x.LastBacktestReturn)
            .On(nameof(GetAllStrategiesResponse.CreatedAt), x => x.CreatedAt)
            .Default(x => x.CreatedAt, SortDirection.Desc);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllStrategiesHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllStrategiesHandler(
        ILogger<GetAllStrategiesHandler> logger,
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

    public async Task<PagedResponse<GetAllStrategiesResponse>> Handle(
        GetAllStrategiesInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all strategies query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        var strategies = await _dbContext.Strategies
            .AsNoTracking()
            .Where(s => s.MarketId == input.MarketId)
            .ToListAsync(cancellationToken);

        var strategyIds = strategies.Select(s => s.Id).ToList();

        var backtestSummaries = await _dbContext.Backtests
            .AsNoTracking()
            .Where(b => strategyIds.Contains(b.StrategyId) && b.Status == BacktestStatus.Completed)
            .GroupBy(b => b.StrategyId)
            .Select(g => new
            {
                StrategyId = g.Key,
                Count = g.Count(),
                LastReturn = g.OrderByDescending(b => b.CreatedAt).Select(b => b.Results!.TotalReturnPercent).FirstOrDefault(),
                LastWinRate = g.OrderByDescending(b => b.CreatedAt).Select(b => b.Results!.WinRate).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.StrategyId, cancellationToken);

        var projected = strategies
            .Select(s => new GetAllStrategiesResponse(
                s.Id,
                s.MarketId,
                s.Name,
                s.Description,
                s.Type,
                s.IsActive,
                s.CreatedAt,
                backtestSummaries.TryGetValue(s.Id, out var summary) ? summary.Count : 0,
                backtestSummaries.TryGetValue(s.Id, out var s2) ? s2.LastReturn : null,
                backtestSummaries.TryGetValue(s.Id, out var s3) ? s3.LastWinRate : null
            ))
            .ToList();

        var filtered = projected.AsQueryable().FilterBy(input.Filter, FilterBuilder);
        var totalCount = filtered.Count();

        var items = filtered
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Paginate(input.Skip, input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get all strategies request.");
        return new PagedResponse<GetAllStrategiesResponse>(items, totalCount, input.Skip, input.Take);
    }
}