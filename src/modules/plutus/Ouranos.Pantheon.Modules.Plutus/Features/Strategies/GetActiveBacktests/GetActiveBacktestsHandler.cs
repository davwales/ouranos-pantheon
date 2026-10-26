using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests;

public sealed class GetActiveBacktestsHandler
    : IPantheonHandler<GetActiveBacktestsInput, PagedResponse<GetActiveBacktestsResponse>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetActiveBacktestsHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetActiveBacktestsHandler(
        ILogger<GetActiveBacktestsHandler> logger,
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

    public async Task<PagedResponse<GetActiveBacktestsResponse>> Handle(
        GetActiveBacktestsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get active backtests query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var query = _dbContext
            .Backtests.AsNoTracking()
            .Where(b => b.Status == BacktestStatus.Pending || b.Status == BacktestStatus.Running);

        if (input.MarketId is { } marketId)
        {
            query = query.Where(b => b.MarketId == marketId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip(input.Skip)
            .Take(input.Take)
            .Join(
                _dbContext.Strategies,
                b => b.StrategyId,
                s => s.Id,
                (b, s) =>
                    new GetActiveBacktestsResponse(
                        b.Id,
                        b.StrategyId,
                        s.Name,
                        b.MarketId,
                        b.Kind,
                        b.Status,
                        b.ProgressPercent,
                        b.ProgressMessage,
                        b.CreatedAt
                    )
            )
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get active backtests request.");
        return new PagedResponse<GetActiveBacktestsResponse>(
            items,
            totalCount,
            input.Skip,
            input.Take
        );
    }
}
