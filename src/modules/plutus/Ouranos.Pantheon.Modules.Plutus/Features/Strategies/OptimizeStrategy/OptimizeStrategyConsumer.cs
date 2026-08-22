using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Pipeline;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
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

    private const double UnderTradingPenaltyPerMissingTrade = 0.1;

    private const double ValidationSharpeRatio = 0.5;

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
    public async Task Handle(
        OptimizeStrategyMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace(
            "Processing optimization for backtest '{backtestId}'.",
            message.BacktestId
        );
        cancellationToken.ThrowIfCancellationRequested();

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var backtest = await dbContext
            .Backtests.Include(b => b.Strategy)
            .FirstOrDefaultAsync(b => b.Id == message.BacktestId, cancellationToken);

        if (backtest is null)
        {
            _logger.LogWarning(
                "Backtest '{backtestId}' not found for optimization.",
                message.BacktestId
            );
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
            await RunOptimizationFlowAsync(backtest, message, dbContext, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Optimization for backtest '{backtestId}' was cancelled.",
                message.BacktestId
            );

            if (backtest.Status is BacktestStatus.Pending or BacktestStatus.Running)
            {
                backtest.Cancel("Cancelled by user.");
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Optimization for backtest '{backtestId}' failed.",
                message.BacktestId
            );
            backtest.Fail(ex.Message);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task RunOptimizationFlowAsync(
        Backtest backtest,
        OptimizeStrategyMessage message,
        PlutusDbContext dbContext,
        CancellationToken cancellationToken
    )
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

        var outSampleRatio = Math.Clamp(message.OutSampleRatio, 0.05, 0.5);
        var totalSpan = backtest.EndDate - backtest.StartDate;
        var inSampleEnd = backtest.StartDate + totalSpan * (1.0 - outSampleRatio);

        var bestChromosome = await RunOptimizationAsync(
            backtest,
            message,
            data,
            inSampleEnd,
            dbContext,
            cancellationToken
        );

        backtest.UpdateProgress(92, "Running final backtest with optimized configuration...");
        await dbContext.SaveChangesAsync(cancellationToken);

        var (inSampleResults, inSamplePositions) = await RunPipelineAsync(
            backtest.Strategy,
            backtest.MarketId,
            backtest.StartDate,
            inSampleEnd,
            backtest.Budget,
            bestChromosome.Configuration,
            data,
            message.VolumeParticipationRate,
            message.SlippageMultiplier,
            bestChromosome,
            cancellationToken
        );
        var (outSampleResults, _) = await RunPipelineAsync(
            backtest.Strategy,
            backtest.MarketId,
            inSampleEnd,
            backtest.EndDate,
            backtest.Budget,
            bestChromosome.Configuration,
            data,
            message.VolumeParticipationRate,
            message.SlippageMultiplier,
            bestChromosome,
            cancellationToken
        );

        var isValidated = ComputeIsValidated(inSampleResults, outSampleResults);

        var finalResults = WithOptimizedConfigs(
            inSampleResults,
            bestChromosome,
            outSampleResults,
            isValidated
        );

        backtest.Complete(finalResults);
        backtest.Positions = inSamplePositions;
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Optimization for backtest '{backtestId}' completed. In-sample Sharpe={isSharpe:F3}, OOS Sharpe={oosSharpe:F3}, validated={isValidated}.",
            message.BacktestId,
            inSampleResults.SharpeRatio,
            outSampleResults.SharpeRatio,
            isValidated
        );
    }

    private static BacktestResults WithOptimizedConfigs(
        BacktestResults results,
        StrategyChromosome chromosome,
        BacktestResults outSampleResults,
        bool isValidated
    )
    {
        return results with
        {
            OptimizedConfiguration = chromosome.Configuration,
            OptimizedInputWeights = NormalizeInputWeights(chromosome.InputWeights),
            OptimizedThresholds = chromosome.Thresholds,
            IsValidated = isValidated,
            OutSampleResults = outSampleResults,
        };
    }

    internal static List<InputWeight> NormalizeInputWeights(List<InputWeight> inputWeights)
    {
        return [.. inputWeights.GroupBy(w => w.Kind).Select(g => g.First()).OrderBy(w => w.Kind)];
    }

    private async Task<StrategyChromosome> RunOptimizationAsync(
        Backtest backtest,
        OptimizeStrategyMessage message,
        BacktestData data,
        DateTimeOffset inSampleEnd,
        PlutusDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var population = Enumerable
            .Range(0, (int)message.PopulationSize)
            .Select(_ => StrategyChromosome.CreateRandom())
            .ToList();

        var fitnessCache = new ConcurrentDictionary<string, double>();

        var engine = new GeneticAlgorithmBuilder<double>()
            .SetElitismRate(_options.Value.ElitismRate)
            .SetMutationRate(_options.Value.MutationRate)
            .SetPopulationSize(message.PopulationSize)
            .AddFitnessComponent(chromosome =>
                ComputeFitnessAsync(
                    chromosome,
                    backtest,
                    data,
                    inSampleEnd,
                    message,
                    fitnessCache,
                    cancellationToken
                )
            )
            .Build();

        var bestChromosome = await engine.EvolveAsync(
            population,
            message.Generations,
            onGenerationCompletedAsync: async (generation, _) =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentStatus = await dbContext
                    .Backtests.AsNoTracking()
                    .Where(b => b.Id == message.BacktestId)
                    .Select(b => b.Status)
                    .FirstOrDefaultAsync(cancellationToken);

                if (currentStatus == BacktestStatus.Cancelled)
                {
                    throw new OperationCanceledException(
                        $"Backtest '{message.BacktestId}' was cancelled."
                    );
                }

                var percent = 5 + (int)(85.0 * generation / message.Generations);
                backtest.UpdateProgress(
                    percent,
                    $"Optimizing: generation {generation + 1}/{message.Generations}..."
                );
                await dbContext.SaveChangesAsync(cancellationToken);
            },
            cancellationToken: cancellationToken
        );

        var bestStrategyChromosome = ExtractStrategyChromosome(bestChromosome);
        var bestFitness = await engine.EvaluateFitnessAsync(bestChromosome);

        if (bestFitness <= double.MinValue / 2)
        {
            _logger.LogWarning(
                "Optimization found no viable configuration. Falling back to original strategy configuration."
            );
            return new StrategyChromosome(
                backtest.Strategy.TradingConfiguration,
                backtest.Strategy.InputWeights,
                backtest.Strategy.Thresholds
            );
        }

        _logger.LogDebug(
            "Optimization completed for backtest '{backtestId}'. Best fitness: {bestFitness}.",
            message.BacktestId,
            bestFitness
        );

        return bestStrategyChromosome;
    }

    private async Task<(
        BacktestResults Results,
        List<BacktestPosition> Positions
    )> RunPipelineAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        TradingConfiguration configurationOverride,
        BacktestData data,
        decimal volumeParticipationRate,
        decimal slippageMultiplier,
        StrategyChromosome chromosome,
        CancellationToken cancellationToken
    )
    {
        var parameters = new BacktestParameters(
            marketId,
            strategy,
            startDate,
            endDate,
            budget,
            volumeParticipationRate,
            slippageMultiplier,
            configurationOverride
        );
        parameters = chromosome.ApplyConfigOverrides(parameters);
        var payload = new BacktestPayload(parameters) { Data = data };

        var backtestPipeline = new PipelineBuilder<BacktestPayload>(_stepRegistry)
            .AddStep<InitializeStep>()
            .AddNestedPipeline(builder =>
                builder
                    .AddStep<IterationSetupStep>()
                    .AddStep<ScoreSymbolsStep>()
                    .AddStep<CloseExitsStep>()
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
            throw new InvalidOperationException(
                "Backtest pipeline completed without producing results."
            );
        }

        return (payload.Results, payload.Portfolio.ClosedPositions);
    }

    private async Task<(
        BacktestResults Results,
        List<BacktestPosition> Positions
    )?> RunBacktestSafelyAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        TradingConfiguration configuration,
        BacktestData data,
        decimal volumeParticipationRate,
        decimal slippageMultiplier,
        StrategyChromosome chromosome,
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
                configuration,
                data,
                volumeParticipationRate,
                slippageMultiplier,
                chromosome,
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(
                ex,
                "Backtest evaluation failed during optimization, returning null fitness."
            );
            return null;
        }
    }

    /// <summary>
    ///     Fitness callback for the genetic engine. Returns the cached fitness for an
    ///     already-evaluated gene signature; otherwise runs the backtest pipeline with
    ///     the chromosome's config, derives the fitness from the result, and caches it
    ///     so repeated gene vectors across generations are scored once. Separated from
    ///     <see cref="RunOptimizationAsync" /> to keep that method readable and to give
    ///     the test layer a direct seam into the cache-backed fitness path.
    /// </summary>
    internal async Task<double> ComputeFitnessAsync(
        IChromosome<double> chromosome,
        Backtest backtest,
        BacktestData data,
        DateTimeOffset inSampleEnd,
        OptimizeStrategyMessage message,
        ConcurrentDictionary<string, double> fitnessCache,
        CancellationToken cancellationToken
    )
    {
        var geneSignature = string.Join(",", chromosome.Genes.Select(g => g.ToString("R")));
        if (fitnessCache.TryGetValue(geneSignature, out var cachedFitness))
        {
            return cachedFitness;
        }

        var strategyChromosome = ExtractStrategyChromosome(chromosome);
        var pipelineResult = await RunBacktestSafelyAsync(
            backtest.Strategy,
            backtest.MarketId,
            backtest.StartDate,
            inSampleEnd,
            backtest.Budget,
            strategyChromosome.Configuration,
            data,
            message.VolumeParticipationRate,
            message.SlippageMultiplier,
            strategyChromosome,
            cancellationToken
        );

        var fitness = pipelineResult is null
            ? double.MinValue
            : ComputeFitness(
                pipelineResult.Value.Results,
                message,
                strategyChromosome.InputWeights
            );

        fitnessCache[geneSignature] = fitness;
        return fitness;
    }

    internal static StrategyChromosome ExtractStrategyChromosome(IChromosome<double> chromosome)
    {
        if (chromosome is StrategyChromosome strategyChromosome)
        {
            return strategyChromosome;
        }

        throw new InvalidOperationException(
            $"Expected {nameof(StrategyChromosome)} but got {chromosome.GetType().Name}."
        );
    }

    internal static double ComputeFitness(
        BacktestResults results,
        OptimizeStrategyMessage message,
        IReadOnlyList<InputWeight> weights
    )
    {
        var sortino = (double)(results.SortinoRatio ?? results.SharpeRatio);
        var cagr = (double)(results.Cagr ?? results.TotalReturnPercent);

        var baseFitness =
            message.SortinoWeight * sortino
            + message.CagrWeight * cagr
            - message.DrawdownWeight * (double)results.MaxDrawdownPercent
            - message.TurnoverWeight * (double)results.TurnoverRate
            - message.L1RegularizationWeight * weights.Sum(w => Math.Abs((double)w.Weight));

        var tradeShortfall = Math.Max(0, message.MinTrades - results.TotalTrades);
        var underTradingPenalty = tradeShortfall * UnderTradingPenaltyPerMissingTrade;
        return baseFitness - underTradingPenalty;
    }

    internal static bool ComputeIsValidated(BacktestResults inSample, BacktestResults outSample)
    {
        if (inSample.SharpeRatio == 0m || outSample.SharpeRatio == 0m)
        {
            return false;
        }

        return (double)outSample.SharpeRatio
            >= ValidationSharpeRatio * (double)inSample.SharpeRatio;
    }
}
