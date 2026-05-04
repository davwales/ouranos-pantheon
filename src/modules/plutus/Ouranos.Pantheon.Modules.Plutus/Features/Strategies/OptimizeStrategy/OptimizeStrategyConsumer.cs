using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
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
    private readonly BacktestDataQueryService _dataService;
    private readonly BacktestEngine _engine;
    private readonly IOptions<OptimizationOptions> _options;

    public OptimizeStrategyConsumer(
        ILogger<OptimizeStrategyConsumer> logger,
        IDbContextFactory<PlutusDbContext> dbContextFactory,
        BacktestDataQueryService dataService,
        BacktestEngine engine,
        IOptions<OptimizationOptions> options
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContextFactory);
        Guard.Against.Null(dataService);
        Guard.Against.Null(engine);
        Guard.Against.Null(options);

        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _dataService = dataService;
        _engine = engine;
        _options = options;
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
            _logger.LogWarning("Backtest '{backtestId}' is already in {status} state. Skipping as duplicate delivery.", message.BacktestId, backtest.Status);
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
                cancellationToken
            );

            var bestConfig = await RunOptimizationAsync(
                backtest.Strategy,
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                backtest.Budget,
                message.Generations,
                message.PopulationSize,
                message.SharpeRatioWeight,
                message.TotalReturnWeight,
                message.MaxDrawdownWeight,
                data,
                cancellationToken
            );

            var results = await _engine.RunAsync(
                backtest.Strategy,
                backtest.MarketId,
                backtest.StartDate,
                backtest.EndDate,
                backtest.Budget,
                cancellationToken,
                bestConfig,
                data
            );

            backtest.Complete(results);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Optimization for backtest '{backtestId}' completed successfully.", message.BacktestId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimization for backtest '{backtestId}' failed.", message.BacktestId);
            backtest.Fail(ex.Message);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<StrategyConfiguration> RunOptimizationAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        uint generations,
        uint populationSize,
        double sharpeRatioWeight,
        double totalReturnWeight,
        double maxDrawdownWeight,
        BacktestData data,
        CancellationToken cancellationToken
    )
    {
        var population = Enumerable
            .Range(0, (int)populationSize)
            .Select(_ => new StrategyConfigurationChromosome(strategy.Type))
            .ToList();

        var engine = new GeneticAlgorithmBuilder<double>()
            .SetElitismRate(_options.Value.ElitismRate)
            .SetMutationRate(_options.Value.MutationRate)
            .SetPopulationSize(populationSize)
            .AddFitnessComponent(async chromosome =>
                {
                    var config = ExtractConfiguration(chromosome);
                    var results = await RunBacktestSafelyAsync(
                        strategy,
                        marketId,
                        startDate,
                        endDate,
                        budget,
                        config,
                        data,
                        cancellationToken
                    );

                    if (results is null)
                    {
                        return double.MinValue;
                    }

                    return sharpeRatioWeight * (double)results.SharpeRatio
                           + totalReturnWeight * (double)results.TotalReturnPercent
                           + maxDrawdownWeight * (double)results.MaxDrawdownPercent;
                }
            )
            .Build();

        var bestChromosome = await engine.EvolveAsync(
            population,
            generations,
            onGenerationCompletedAsync: (generation, _) =>
            {
                if (generation % 10 == 0)
                {
                    _logger.LogDebug("Optimization progress: generation {generation}.", generation);
                }

                return Task.CompletedTask;
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
            return strategy.Configuration;
        }

        return bestConfig;
    }

    private async Task<BacktestResults?> RunBacktestSafelyAsync(
        Strategy strategy,
        Id<Market> marketId,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        decimal budget,
        StrategyConfiguration configuration,
        BacktestData data,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await _engine.RunAsync(
                strategy,
                marketId,
                startDate,
                endDate,
                budget,
                cancellationToken,
                configuration,
                data
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
