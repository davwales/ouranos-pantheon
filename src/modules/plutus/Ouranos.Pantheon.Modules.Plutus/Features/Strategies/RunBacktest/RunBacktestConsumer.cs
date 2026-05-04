using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumer : IPantheonHandler<RunBacktestMessage>
{
    private readonly ILogger<RunBacktestConsumer> _logger;
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly BacktestEngine _engine;

    public RunBacktestConsumer(
        ILogger<RunBacktestConsumer> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        BacktestEngine engine
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(engine);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _engine = engine;
    }

    public async Task Handle(RunBacktestMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Processing backtest '{backtestId}'.", message.BacktestId);
        cancellationToken.ThrowIfCancellationRequested();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var backtest = await dbContext.Backtests
            .Include(b => b.Strategy)
            .FirstOrDefaultAsync(b => b.Id == message.BacktestId, cancellationToken);

        if (backtest is null)
        {
            _logger.LogWarning("Backtest '{backtestId}' not found.", message.BacktestId);
            return;
        }

        if (backtest.Status != BacktestStatus.Pending)
        {
            _logger.LogWarning("Backtest '{backtestId}' is already in {status} state. Skipping as duplicate delivery.", message.BacktestId, backtest.Status);
            return;
        }

        backtest.MarkRunning();
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var data = await _engine.LoadDataAsync(
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                cancellationToken
            );

            var results = await _engine.RunAsync(
                backtest.Strategy,
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                backtest.Budget,
                cancellationToken,
                data: data
            );

            backtest.Complete(results);
            await dbContext.SaveChangesAsync(cancellationToken);

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
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
