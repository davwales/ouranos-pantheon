using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;
using Wolverine.Attributes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;

public sealed class RunBacktestConsumer : IPantheonHandler<RunBacktestMessage>
{
    private readonly ILogger<RunBacktestConsumer> _logger;
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IBacktestDataQueryService _dataService;
    private readonly IOptions<BacktestDataOptions> _backtestDataOptions;
    private readonly IStepRegistry<BacktestPayload> _stepRegistry;

    public RunBacktestConsumer(
        ILogger<RunBacktestConsumer> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        IBacktestDataQueryService dataService,
        IOptions<BacktestDataOptions> backtestDataOptions,
        IStepRegistry<BacktestPayload> stepRegistry
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(dataService);
        Guard.Against.Null(backtestDataOptions);
        Guard.Against.Null(stepRegistry);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _dataService = dataService;
        _backtestDataOptions = backtestDataOptions;
        _stepRegistry = stepRegistry;
    }

    [MessageTimeout(3600)]
    public async Task Handle(
        RunBacktestMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Processing backtest '{backtestId}'.", message.BacktestId);
        cancellationToken.ThrowIfCancellationRequested();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var backtest = await dbContext
            .Backtests.Include(b => b.Strategy)
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
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Backtest '{backtestId}' was claimed by a concurrent delivery. Skipping.",
                message.BacktestId
            );
            return;
        }

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

            var totalDays = (int)(backtest.EndDate - backtest.StartDate).TotalDays;

            var payload = new BacktestPayload(
                new BacktestParameters(
                    backtest.MarketId,
                    backtest.Strategy,
                    backtest.StartDate,
                    backtest.EndDate,
                    backtest.Budget,
                    message.VolumeParticipationRate,
                    message.SlippageMultiplier
                )
            )
            {
                Data = data,
                Entity = backtest,
                ProgressInterval = Math.Max(1, totalDays / 20),
            };

            var backtestPipeline = new PipelineBuilder<BacktestPayload>(_stepRegistry)
                .AddStep<InitializeStep>()
                .AddNestedPipeline(builder =>
                    builder
                        .AddStep<IterationSetupStep>()
                        .AddStep<ScoreSymbolsStep>()
                        .AddStep<CloseExitsStep>()
                        .AddStep<BuyCandidatesStep>()
                        .AddStep<TrackMetricsStep>()
                        .WithIterations(totalDays + 1)
                        .Build()
                )
                .AddStep<LiquidateStep>()
                .AddStep<ComputeResultsStep>()
                .Build();

            var rootContext = new PipelineContext(cancellationToken);
            await backtestPipeline.ExecuteAsync(rootContext, payload);

            if (payload.Results is null)
            {
                throw new InvalidOperationException(
                    "Backtest pipeline completed without producing results."
                );
            }

            backtest.Complete(payload.Results);
            backtest.Positions = payload.Portfolio.ClosedPositions;
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
