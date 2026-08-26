using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest;

public sealed class GetBacktestHandler : IPantheonHandler<GetBacktestInput, GetBacktestResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetBacktestHandler> _logger;

    public GetBacktestHandler(ILogger<GetBacktestHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetBacktestResponse> Handle(
        GetBacktestInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get backtest query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var backtest = await _dbContext
            .Backtests.AsNoTracking()
            .Include(b => b.Positions.OrderByDescending(p => p.EntryTime))
            .FirstOrDefaultAsync(b => b.Id == query.BacktestId, cancellationToken);

        Guard.Against.NotFound(query.BacktestId, backtest);

        _logger.LogDebug("Successfully handled get backtest request.");
        return new GetBacktestResponse(
            backtest.Id,
            backtest.StrategyId,
            backtest.MarketId,
            backtest.StartDate,
            backtest.EndDate,
            backtest.Budget,
            backtest.Kind,
            backtest.Status,
            backtest.ProgressPercent,
            backtest.ProgressMessage,
            backtest.Results,
            backtest.Positions,
            backtest.ErrorMessage,
            backtest.CreatedAt,
            backtest.UpdatedAt
        );
    }
}
