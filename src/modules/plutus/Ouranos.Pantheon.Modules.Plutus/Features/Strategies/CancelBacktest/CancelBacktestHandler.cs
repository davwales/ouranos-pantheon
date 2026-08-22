using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest;

public sealed class CancelBacktestHandler
    : IPantheonHandler<CancelBacktestInput, CancelBacktestResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<CancelBacktestHandler> _logger;

    public CancelBacktestHandler(ILogger<CancelBacktestHandler> logger, PlutusDbContext dbContext)
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CancelBacktestResponse> Handle(
        CancelBacktestInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle cancel backtest command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        var backtest = await _dbContext.Backtests.FirstOrDefaultAsync(
            b => b.Id == command.BacktestId,
            cancellationToken
        );

        Guard.Against.NotFound(command.BacktestId, backtest);

        backtest.Cancel(command.Reason);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully cancelled backtest '{backtestId}'.", backtest.Id);
        return new CancelBacktestResponse(backtest.Id, backtest.Status);
    }
}
