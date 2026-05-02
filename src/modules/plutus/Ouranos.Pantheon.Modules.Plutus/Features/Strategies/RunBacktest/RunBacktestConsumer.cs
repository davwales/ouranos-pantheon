using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumer : IPantheonHandler<RunBacktestMessage>
{
    private readonly ILogger<RunBacktestConsumer> _logger;
    private readonly PlutusDbContext _dbContext;
    private readonly BacktestEngine _engine;

    public RunBacktestConsumer(
        ILogger<RunBacktestConsumer> logger,
        PlutusDbContext dbContext,
        BacktestEngine engine
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(engine);

        _logger = logger;
        _dbContext = dbContext;
        _engine = engine;
    }

    public async Task Handle(RunBacktestMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Processing backtest '{backtestId}'.", message.BacktestId);
        cancellationToken.ThrowIfCancellationRequested();

        var backtest = await _dbContext.Backtests
            .Include(b => b.Strategy)
            .FirstOrDefaultAsync(b => b.Id == message.BacktestId, cancellationToken);

        if (backtest is null)
        {
            _logger.LogWarning("Backtest '{backtestId}' not found.", message.BacktestId);
            return;
        }

        try
        {
            backtest.MarkRunning();
            await _dbContext.SaveChangesAsync(cancellationToken);

            var results = await _engine.RunAsync(
                backtest.Strategy,
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                backtest.Budget,
                cancellationToken
            );

            backtest.Complete(results);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Backtest '{backtestId}' completed successfully.", message.BacktestId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backtest '{backtestId}' failed.", message.BacktestId);
            backtest.Fail(ex.Message);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}