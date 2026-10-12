using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;

/// <summary>
///     Evolves a strategy's parameters on the in-sample part of the window, then reruns the
///     winner on both parts. A candidate is scored on every <see cref="IStrategyEngine.OptimizationSegmentDays" />
///     slice of the in-sample window so it has to work throughout, not just in one lucky stretch.
/// </summary>
public sealed class StrategyOptimizer(
    IOptions<OptimizationOptions> options,
    ILogger<StrategyOptimizer> logger
)
{
    private const int SearchProgressStart = 5;
    private const int SearchProgressEnd = 90;
    private const int FinalRunProgress = 92;

    private readonly OptimizationOptions _options = Guard.Against.Null(options).Value;
    private readonly ILogger<StrategyOptimizer> _logger = Guard.Against.Null(logger);

    public async Task<BacktestOutcome> OptimizeAsync(
        IStrategyEngine engine,
        IStrategyMarketData data,
        Backtest backtest,
        IRunMonitor monitor
    )
    {
        Guard.Against.Null(engine);
        Guard.Against.Null(data);
        Guard.Against.Null(backtest);
        Guard.Against.Null(monitor);

        var settings = Guard.Against.Null(backtest.RunSettings.Optimization);
        var inSampleEnd = OptimizationFitness.InSampleEnd(
            backtest.StartDate,
            backtest.EndDate,
            settings.OutSampleRatio
        );

        var best = await SearchAsync(engine, data, backtest, settings, inSampleEnd, monitor);

        monitor.Report(FinalRunProgress, "Running final backtest with optimized configuration...");
        var inSample = Settings(backtest, backtest.StartDate, inSampleEnd, monitor);
        var outSample = Settings(backtest, inSampleEnd, backtest.EndDate, monitor);
        var inSampleRun = engine.Simulate(data, best, inSample);
        var inSampleResults = inSampleRun.ToResults(inSample);
        var outSampleResults = engine.Simulate(data, best, outSample).ToResults(outSample);
        var isValidated = OptimizationFitness.IsValidated(inSampleResults, outSampleResults);

        _logger.LogDebug(
            "Optimization for backtest '{backtestId}' completed. In-sample return={isReturn:P2}, OOS return={oosReturn:P2}, validated={isValidated}.",
            backtest.Id,
            inSampleResults.TotalReturnPercent,
            outSampleResults.TotalReturnPercent,
            isValidated
        );

        return new BacktestOutcome(
            inSampleResults with
            {
                OptimizedConfiguration = best,
                IsValidated = isValidated,
                OutSampleResults = outSampleResults,
            },
            inSampleRun.Positions
        );
    }

    private async Task<StrategyConfiguration> SearchAsync(
        IStrategyEngine engine,
        IStrategyMarketData data,
        Backtest backtest,
        OptimizationSettings settings,
        DateTimeOffset inSampleEnd,
        IRunMonitor monitor
    )
    {
        var space = engine.CreateParameterSpace(backtest.Strategy.Configuration);

        var segments = OptimizationFitness.BuildSegments(
            backtest.StartDate,
            inSampleEnd,
            engine.OptimizationSegmentDays
        );

        var minTrades = OptimizationFitness.MinTradesForSegment(
            settings.MinTradesPerMonth,
            engine.OptimizationSegmentDays
        );

        var cache = new ConcurrentDictionary<string, double>();

        Task<double> Fitness(IChromosome<double> chromosome)
        {
            return cache.TryGetValue(Signature(chromosome), out var cached)
                ? Task.FromResult(cached)
                : EvaluateAsync(chromosome);
        }

        async Task<double> EvaluateAsync(IChromosome<double> chromosome)
        {
            var configuration = ((ParameterChromosome)chromosome).Configuration;
            var segmentFitness = await Task.WhenAll(
                segments.Select(segment =>
                    Task.Run(
                        () =>
                            SegmentFitness(
                                engine,
                                data,
                                configuration,
                                Settings(backtest, segment.Start, segment.End, monitor),
                                settings,
                                minTrades
                            ),
                        monitor.CancellationToken
                    )
                )
            );

            var fitness = segmentFitness.Any(f => f is null)
                ? double.MinValue
                : OptimizationFitness.CombineSegments([
                    .. segmentFitness.Select(f => f.GetValueOrDefault()),
                ]);

            cache[Signature(chromosome)] = fitness;
            return fitness;
        }

        var seed = new ParameterChromosome(space);
        List<IChromosome<double>> population =
        [
            seed,
            .. Enumerable
                .Range(1, settings.PopulationSize - 1)
                .Select(_ => ParameterChromosome.CreateRandom(space)),
        ];

        var algorithm = new GeneticAlgorithmBuilder<double>()
            .SetElitismRate(_options.ElitismRate)
            .SetMutationRate(_options.MutationRate)
            .SetPopulationSize((uint)settings.PopulationSize)
            .AddFitnessComponent(Fitness)
            .Build();

        var best = await algorithm.EvolveAsync(
            population,
            (uint)settings.Generations,
            onGenerationCompletedAsync: (generation, _) =>
            {
                monitor.Report(
                    SearchProgressStart
                        + (
                            (SearchProgressEnd - SearchProgressStart)
                            * (generation + 1)
                            / settings.Generations
                        ),
                    $"Optimizing: generation {generation + 1}/{settings.Generations}..."
                );
                return Task.CompletedTask;
            },
            cancellationToken: monitor.CancellationToken
        );

        if (await algorithm.EvaluateFitnessAsync(best) <= double.MinValue / 2)
        {
            _logger.LogWarning(
                "Optimization for backtest '{backtestId}' found no viable configuration; keeping the original.",
                backtest.Id
            );
            return backtest.Strategy.Configuration;
        }

        return ((ParameterChromosome)best).Configuration;
    }

    private double? SegmentFitness(
        IStrategyEngine engine,
        IStrategyMarketData data,
        StrategyConfiguration configuration,
        SimulationSettings segment,
        OptimizationSettings settings,
        int minTrades
    )
    {
        try
        {
            var run = engine.Simulate(data, configuration, segment);
            return OptimizationFitness.Compute(
                run.ToResults(segment),
                run.Positions,
                segment.Budget,
                settings.DrawdownWeight,
                minTrades
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Optimization candidate failed to simulate.");
            return null;
        }
    }

    private static SimulationSettings Settings(
        Backtest backtest,
        DateTimeOffset start,
        DateTimeOffset end,
        IRunMonitor monitor
    )
    {
        return new SimulationSettings(
            start,
            end,
            backtest.Budget,
            backtest.RunSettings.VolumeParticipationRate,
            backtest.RunSettings.SlippageMultiplier,
            CancellationToken: monitor.CancellationToken
        );
    }

    private static string Signature(IChromosome<double> chromosome)
    {
        return string.Join(",", chromosome.Genes.Select(g => g.ToString("R")));
    }
}
