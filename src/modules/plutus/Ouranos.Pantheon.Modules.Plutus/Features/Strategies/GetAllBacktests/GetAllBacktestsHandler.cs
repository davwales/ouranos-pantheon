using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Common.Sorting;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests;

public sealed class GetAllBacktestsHandler
    : IPantheonHandler<GetAllBacktestsInput, PagedResponse<GetAllBacktestsResponse>>
{
    private static readonly SortBuilder<GetAllBacktestsResponse> SortBuilder =
        new SortBuilder<GetAllBacktestsResponse>()
            .On(nameof(GetAllBacktestsResponse.CreatedAt), x => x.CreatedAt)
            .On(nameof(GetAllBacktestsResponse.Kind), x => x.Kind)
            .On(nameof(GetAllBacktestsResponse.TotalReturnPercent), x => x.TotalReturnPercent ?? 0)
            .On(nameof(GetAllBacktestsResponse.WinRate), x => x.WinRate ?? 0)
            .On(nameof(GetAllBacktestsResponse.SharpeRatio), x => x.SharpeRatio ?? 0)
            .Default(x => x.CreatedAt);

    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetAllBacktestsHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetAllBacktestsHandler(
        ILogger<GetAllBacktestsHandler> logger,
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

    public async Task<PagedResponse<GetAllBacktestsResponse>> Handle(
        GetAllBacktestsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get all backtests query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(input.Take, nameof(input.Take), limits.MinPageSize, limits.MaxPageSize);

        var backtests = await _dbContext.Backtests
            .AsNoTracking()
            .Where(b => b.StrategyId == input.StrategyId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new GetAllBacktestsResponse(
                    b.Id,
                    b.MarketId,
                    b.StartDate,
                    b.EndDate,
                    b.Budget,
                    b.Kind,
                    b.Status,
                    b.Results != null ? b.Results.TotalReturnPercent : null,
                    b.Results != null ? b.Results.WinRate : null,
                    b.Results != null ? b.Results.SharpeRatio : null,
                    b.Results != null ? b.Results.TotalTrades : null,
                    b.CreatedAt
                )
            )
            .ToListAsync(cancellationToken);

        var totalCount = backtests.Count;

        var sorted = backtests.AsQueryable()
            .SortBy(input.SortField, input.SortDirection, SortBuilder)
            .Skip(input.Skip)
            .Take(input.Take)
            .ToList();

        _logger.LogDebug("Successfully handled get all backtests request.");
        return new PagedResponse<GetAllBacktestsResponse>(sorted, totalCount, input.Skip, input.Take);
    }
}