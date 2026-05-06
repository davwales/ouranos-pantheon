using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Wolverine.Attributes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumer : IPantheonHandler<RunBacktestMessage>
{
    private const int MinimumProgressUpdate = 1;

    private readonly ILogger<RunBacktestConsumer> _logger;
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IBacktestDataQueryService _dataService;
    private readonly BacktestEngine _engine;
    private readonly IOptions<BacktestDataOptions> _backtestDataOptions;

    public RunBacktestConsumer(
        ILogger<RunBacktestConsumer> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        IBacktestDataQueryService dataService,
        BacktestEngine engine,
        IOptions<BacktestDataOptions> backtestDataOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(dataService);
        Guard.Against.Null(engine);
        Guard.Against.Null(backtestDataOptions);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _dataService = dataService;
        _engine = engine;
        _backtestDataOptions = backtestDataOptions;
    }

    [MessageTimeout(3600)]
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
            _logger.LogWarning(
                "Backtest '{backtestId}' is in {status} state. Skipping.",
                message.BacktestId,
                backtest.Status
            );
            return;
        }

        backtest.MarkRunning();
        await dbContext.SaveChangesAsync(cancellationToken);

        var lastSavedPercent = 0;

        try
        {
            backtest.UpdateProgress(1, "Loading market data...");
            await dbContext.SaveChangesAsync(cancellationToken);

            var data = await _dataService.LoadDataAsync(
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                cancellationToken,
                lookbackDays: _backtestDataOptions.Value.LookbackDays
            );

            backtest.UpdateProgress(5, "Market data loaded, starting simulation...");
            await dbContext.SaveChangesAsync(cancellationToken);

            var results = await _engine.RunAsync(
                backtest.Strategy,
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                backtest.Budget,
                cancellationToken,
                data: data,
                volumeParticipationRate: message.VolumeParticipationRate,
                slippageMultiplier: message.SlippageMultiplier,
                onCheckpoint: async (percent, progressMessage) =>
                {
                    if (percent - lastSavedPercent < MinimumProgressUpdate)
                    {
                        return;
                    }

                    lastSavedPercent = percent;

                    var currentStatus = await dbContext.Backtests
                        .AsNoTracking()
                        .Where(b => b.Id == backtest.Id)
                        .Select(b => b.Status)
                        .FirstOrDefaultAsync(CancellationToken.None);

                    if (currentStatus == BacktestStatus.Cancelled)
                    {
                        throw new OperationCanceledException($"Backtest '{backtest.Id}' was cancelled.");
                    }

                    backtest.UpdateProgress(percent, progressMessage);

                    try
                    {
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to save progress for backtest '{backtestId}'.",
                            message.BacktestId
                        );
                    }
                }
            );

            backtest.Complete(results);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Backtest '{backtestId}' completed successfully.", message.BacktestId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Backtest '{backtestId}' was cancelled.", message.BacktestId);

            if (backtest.Status is BacktestStatus.Pending or BacktestStatus.Running)
            {
                backtest.Cancel("Cancelled by user.");
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backtest '{backtestId}' failed.", message.BacktestId);

            if (backtest.Status is BacktestStatus.Pending or BacktestStatus.Running)
            {
                backtest.Fail(ex.Message);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}