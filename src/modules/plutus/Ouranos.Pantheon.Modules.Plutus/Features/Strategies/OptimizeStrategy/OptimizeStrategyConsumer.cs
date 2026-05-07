using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Application.Pipeline;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine.Attributes;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;

public sealed class OptimizeStrategyConsumer : IPantheonHandler<OptimizeStrategyMessage>
{
    private readonly ILogger<OptimizeStrategyConsumer> _logger;
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly IBacktestDataQueryService _dataService;
    private readonly IOptions<OptimizationOptions> _options;
    private readonly IOptions<BacktestDataOptions> _backtestDataOptions;
    private readonly IStepRegistry<BacktestPayload> _stepRegistry;

    public OptimizeStrategyConsumer(
        ILogger<OptimizeStrategyConsumer> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        IBacktestDataQueryService dataService,
        IOptions<OptimizationOptions> options,
        IOptions<BacktestDataOptions> backtestDataOptions,
        IStepRegistry<BacktestPayload> stepRegistry
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(dataService);
        Guard.Against.Null(options);
        Guard.Against.Null(backtestDataOptions);
        Guard.Against.Null(stepRegistry);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _dataService = dataService;
        _options = options;
        _backtestDataOptions = backtestDataOptions;
        _stepRegistry = stepRegistry;
    }

    [MessageTimeout(3600)]
    public async Task Handle(OptimizeStrategyMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Processing optimization for backtest '{backtestId}'.", message.BacktestId);
        cancellationToken.ThrowIfCancellationRequested();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var backtest = await dbContext.Backtests
            .Include(b => b.Strategy)
            .FirstOrDefaultAsync(b => b.Id == message.BacktestId, cancellationToken);

        if (backtest is null)
        {
            _logger.LogWarning("Backtest '{backtestId}' not found for optimization.", message.BacktestId);
            return;
        }

        if (backtest.Status != BacktestStatus.Pending)
        {
            _logger.LogWarning(
                "Backtest '{backtestId}' is already in {status} state. Skipping as duplicate delivery.",
                message.BacktestId,
                backtest.Status
            );
            return;
        }

        backtest.MarkRunning();
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var data = await _dataService.LoadDataAsync(
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                cancellationToken,
                lookbackDays: _backtestDataOptions.Value.LookbackDays
            );

            backtest.UpdateProgress(2, "Market data loaded, starting optimization...");
            await dbContext.SaveChangesAsync(cancellationToken);

            var bestConfig = await RunOptimizationAsync(
                backtest,
                message,
                data,
                dbContext,
                cancellationToken
            );

            backtest.UpdateProgress(92, "Running final backtest with optimized configuration...");
            await dbContext.SaveChangesAsync(cancellationToken);

            var results = await RunPipelineAsync(
                backtest.Strategy,
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                backtest.Budget,
                cancellationToken,
                bestConfig,
                data,
                message.VolumeParticipationRate,
                message.SlippageMultiplier
            );

            var finalResults = results with { OptimizedConfiguration = bestConfig };

            backtest.Complete(finalResults);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Optimization for backtest '{backtestId}' completed successfully.", message.BacktestId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Optimization for backtest '{backtestId}' was cancelled.", message.BacktestId);

            if (backtest.Status is BacktestStatus.Pending or BacktestStatus.Running)
            {
                backtest.Cancel("Cancelled by user.");
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimization for backtest '{backtestId}' failed.", message.BacktestId);
            backtest.Fail(ex.Message);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task<StrategyConfiguration> RunOptimizationAsync(
        Backtest backtest,
        OptimizeStrategyMessage message,
        BacktestData data,
        PlutusDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var population = Enumerable
            .Range(0, (int)message.PopulationSize)
            .Select(_ => new StrategyConfigurationChromosome(backtest.Strategy.Type))
            .ToList();

        var engine = new GeneticAlgorithmBuilder<double>()
            .SetElitismRate(_options.Value.ElitismRate)
            .SetMutationRate(_options.Value.MutationRate)
            .SetPopulationSize(message.PopulationSize)
            .AddFitnessComponent(async chromosome =>
                {
                    var config = ExtractConfiguration(chromosome);
                    var results = await RunBacktestSafelyAsync(
                        backtest.Strategy,
                        backtest.MarketId,
                        backtest.StartDate,
                        backtest.EndDate,
                        backtest.Budget,
                        config,
                        data,
                        message.VolumeParticipationRate,
                        message.SlippageMultiplier,
                        cancellationToken
                    );

                    if (results is null)
                    {
                        return double.MinValue;
                    }

                    return message.SharpeRatioWeight * (double)results.SharpeRatio
                           + message.TotalReturnWeight * (double)results.TotalReturnPercent
                           + message.MaxDrawdownWeight * (double)results.MaxDrawdownPercent;
                }
            )
            .Build();

        var bestChromosome = await engine.EvolveAsync(
            population,
            message.Generations,
            onGenerationCompletedAsync: async (generation, _) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentStatus = await dbContext.Backtests
                    .AsNoTracking()
                    .Where(b => b.Id == message.BacktestId)
                    .Select(b => b.Status)
                    .FirstOrDefaultAsync(cancellationToken);

                if (currentStatus == BacktestStatus.Cancelled)
                {
                    throw new OperationCanceledException($"Backtest '{message.BacktestId}' was cancelled.");
                }

                var percent = 5 + (int)(85.0 * generation / message.Generations);
                backtest.UpdateProgress(percent, $"Optimizing: generation {generation + 1}/{message.Generations}...");
                await dbContext.SaveChangesAsync(cancellationToken);
            },
            cancellationToken: cancellationToken
        );

        var bestConfig = ExtractConfiguration(bestChromosome);
        var bestFitness = await engine.EvaluateFitnessAsync(bestChromosome);

        if (bestFitness <= double.MinValue / 2)
        {
            _logger.LogWarning(
                "Optimization found no viable configuration. Falling back to original strategy configuration."
            );
            return backtest.Strategy.Configuration;
        }

        _logger.LogDebug(
            "Optimization completed for backtest '{backtestId}'. Best fitness: {bestFitness}.",
            message.BacktestId,
            bestFitness
        );

        return bestConfig;
    }

    private async Task<BacktestResults> RunPipelineAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        CancellationToken cancellationToken,
        StrategyConfiguration configurationOverride,
        BacktestData data,
        decimal volumeParticipationRate,
        decimal slippageMultiplier
    )
    {
        var payload = new BacktestPayload(
            new BacktestParameters(
                marketId,
                strategy,
                startDate,
                endDate,
                budget,
                volumeParticipationRate,
                slippageMultiplier,
                configurationOverride
            )
        )
        { Data = data };

        var backtestPipeline = new PipelineBuilder<BacktestPayload>(_stepRegistry)
            .AddStep<InitializeStep>()
            .AddNestedPipeline(builder => builder
                .AddStep<CloseExitsStep>()
                .AddStep<ScoreSymbolsStep>()
                .AddStep<BuyCandidatesStep>()
                .AddStep<TrackMetricsStep>()
                .WithIterations((int)(endDate - startDate).TotalDays + 1)
                .Build()
            )
            .AddStep<LiquidateStep>()
            .AddStep<ComputeResultsStep>()
            .Build();

        var rootContext = new PipelineContext(cancellationToken);
        await backtestPipeline.ExecuteAsync(rootContext, payload);

        if (payload.Results is null)
        {
            throw new InvalidOperationException("Backtest pipeline completed without producing results.");
        }

        return payload.Results;
    }

    private async Task<BacktestResults?> RunBacktestSafelyAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        StrategyConfiguration configuration,
        BacktestData data,
        decimal volumeParticipationRate,
        decimal slippageMultiplier,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await RunPipelineAsync(
                strategy,
                marketId,
                startDate,
                endDate,
                budget,
                cancellationToken,
                configuration,
                data,
                volumeParticipationRate: volumeParticipationRate,
                slippageMultiplier: slippageMultiplier
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Backtest evaluation failed during optimization, returning null fitness.");
            return null;
        }
    }

    private static StrategyConfiguration ExtractConfiguration(IChromosome<double> chromosome)
    {
        if (chromosome is StrategyConfigurationChromosome configChromosome)
        {
            return configChromosome.Configuration;
        }

        throw new InvalidOperationException(
            $"Expected {nameof(StrategyConfigurationChromosome)} but got {chromosome.GetType().Name}."
        );
    }
}