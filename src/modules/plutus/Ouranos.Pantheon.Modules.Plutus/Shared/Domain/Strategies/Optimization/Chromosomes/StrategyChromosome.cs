using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

/// <summary>
///     Chromosome for the signals-only strategy model. Genes encode the three common
///     <see cref="TradingConfiguration" /> fields, one signed weight per input kind the
///     strategy uses (in enum order), and the buy/sell thresholds. Only the strategy's own
///     inputs are evolved, so optimizing a strategy built on a chosen subset of signals tunes
///     exactly those signals. Negative weights let the search use an input contrarily.
/// </summary>
public sealed class StrategyChromosome : IChromosome<double>
{
    public const decimal MaxAbsWeight = 3m;
    public const decimal MinBuyThreshold = 0m;
    public const decimal MaxBuyThreshold = 0.95m;
    public const decimal MinSellThreshold = -0.9m;
    public const decimal MaxSellThreshold = 0.3m;
    public const decimal MinPositionPercent = 0.01m;
    public const decimal MaxPositionPercent = 0.15m;
    public const int MinPositions = 8;
    public const int MaxPositionsLimit = 60;
    public const int MaxHoldPeriodDays = 60;

    public StrategyChromosome(
        TradingConfiguration configuration,
        List<InputWeight> inputWeights,
        InputThresholds thresholds
    )
        : this(configuration, inputWeights, thresholds, [.. inputWeights.Select(w => w.Kind)]) { }

    private StrategyChromosome(
        TradingConfiguration configuration,
        List<InputWeight> inputWeights,
        InputThresholds thresholds,
        IReadOnlyList<InputKind> activeKinds
    )
    {
        ActiveKinds = [.. activeKinds.Distinct().Order()];
        Configuration = configuration;
        InputWeights = NormalizeWeights(inputWeights, ActiveKinds);
        Thresholds = thresholds;
    }

    public IReadOnlyList<InputKind> ActiveKinds { get; }

    public TradingConfiguration Configuration { get; private set; }

    public List<InputWeight> InputWeights { get; private set; }

    public InputThresholds Thresholds { get; private set; }

    public double[] Genes => ToGenes();

    public static StrategyChromosome CreateRandom(IReadOnlyList<InputKind> activeKinds)
    {
        var random = Random.Shared;
        var configuration = new TradingConfiguration
        {
            MaxPositions = random.Next(MinPositions, MaxPositionsLimit + 1),
            MaxPositionPercent = Uniform(random, MinPositionPercent, MaxPositionPercent),
            HoldPeriodDays = random.Next(3, MaxHoldPeriodDays + 1),
        };

        var weights = activeKinds
            .Select(kind => new InputWeight(kind, Uniform(random, -2m, 2m)))
            .ToList();

        var thresholds = new InputThresholds
        {
            BuyThreshold = Uniform(random, MinBuyThreshold, MaxBuyThreshold),
            SellThreshold = Uniform(random, MinSellThreshold, MaxSellThreshold),
        };

        return new StrategyChromosome(configuration, weights, thresholds, activeKinds);
    }

    public BacktestParameters ApplyConfigOverrides(BacktestParameters parameters)
    {
        return parameters with
        {
            ConfigurationOverride = Configuration,
            InputWeightsOverride = InputWeights,
            ThresholdsOverride = Thresholds,
        };
    }

    /// <summary>
    ///     Each gene mutates independently with probability <paramref name="mutationRate" />
    ///     (at least one gene per call on average), by a step proportional to its range.
    /// </summary>
    public void Mutate(double mutationRate)
    {
        var random = Random.Shared;
        var geneCount = 5 + ActiveKinds.Count;
        var probability = Math.Max(mutationRate, 1.0 / geneCount);

        bool Roll()
        {
            return random.NextDouble() < probability;
        }

        var configuration = Configuration;
        if (Roll())
        {
            configuration = configuration with
            {
                MaxPositions = Math.Clamp(
                    (configuration.MaxPositions ?? 10) + random.Next(-3, 4),
                    MinPositions,
                    MaxPositionsLimit
                ),
            };
        }

        if (Roll())
        {
            configuration = configuration with
            {
                MaxPositionPercent = Step(
                    random,
                    configuration.MaxPositionPercent ?? 0.1m,
                    MinPositionPercent,
                    MaxPositionPercent
                ),
            };
        }

        if (Roll())
        {
            configuration = configuration with
            {
                HoldPeriodDays = Math.Clamp(
                    (configuration.HoldPeriodDays ?? 14) + random.Next(-5, 6),
                    1,
                    MaxHoldPeriodDays
                ),
            };
        }

        Configuration = configuration;

        InputWeights =
        [
            .. InputWeights.Select(w =>
                Roll() ? w with { Weight = Step(random, w.Weight, -MaxAbsWeight, MaxAbsWeight) } : w
            ),
        ];

        var thresholds = Thresholds;
        if (Roll())
        {
            thresholds = thresholds with
            {
                BuyThreshold = Step(
                    random,
                    thresholds.BuyThreshold ?? 0m,
                    MinBuyThreshold,
                    MaxBuyThreshold
                ),
            };
        }

        if (Roll())
        {
            thresholds = thresholds with
            {
                SellThreshold = Step(
                    random,
                    thresholds.SellThreshold ?? 0m,
                    MinSellThreshold,
                    MaxSellThreshold
                ),
            };
        }

        Thresholds = thresholds;
    }

    public IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not StrategyChromosome otherChromosome)
        {
            throw new InvalidOperationException(
                $"Crossover partner must be a {nameof(StrategyChromosome)}."
            );
        }

        var random = Random.Shared;

        T Pick<T>(T mine, T theirs)
        {
            return random.NextDouble() < 0.5 ? mine : theirs;
        }

        var childConfig = new TradingConfiguration
        {
            MaxPositions = Pick(
                Configuration.MaxPositions,
                otherChromosome.Configuration.MaxPositions
            ),
            MaxPositionPercent = Pick(
                Configuration.MaxPositionPercent,
                otherChromosome.Configuration.MaxPositionPercent
            ),
            HoldPeriodDays = Pick(
                Configuration.HoldPeriodDays,
                otherChromosome.Configuration.HoldPeriodDays
            ),
        };

        var childWeights = ActiveKinds
            .Select(kind => new InputWeight(
                kind,
                Pick(GetWeight(InputWeights, kind), GetWeight(otherChromosome.InputWeights, kind))
            ))
            .ToList();

        var childThresholds = new InputThresholds
        {
            BuyThreshold = Pick(Thresholds.BuyThreshold, otherChromosome.Thresholds.BuyThreshold),
            SellThreshold = Pick(
                Thresholds.SellThreshold,
                otherChromosome.Thresholds.SellThreshold
            ),
        };

        return new StrategyChromosome(childConfig, childWeights, childThresholds, ActiveKinds);
    }

    private double[] ToGenes()
    {
        var genes = new List<double>
        {
            Configuration.MaxPositions ?? 0,
            (double)(Configuration.MaxPositionPercent ?? 0m),
            Configuration.HoldPeriodDays ?? 0,
        };
        genes.AddRange(InputWeights.Select(w => (double)w.Weight));
        genes.Add((double)(Thresholds.BuyThreshold ?? 0m));
        genes.Add((double)(Thresholds.SellThreshold ?? 0m));
        return [.. genes];
    }

    private static decimal Uniform(Random random, decimal min, decimal max)
    {
        return min + (decimal)random.NextDouble() * (max - min);
    }

    /// <summary>Moves a value by up to ±15% of its range, clamped to the range.</summary>
    private static decimal Step(Random random, decimal value, decimal min, decimal max)
    {
        var step = (decimal)(random.NextDouble() * 2 - 1) * 0.15m * (max - min);
        return Math.Clamp(value + step, min, max);
    }

    private static List<InputWeight> NormalizeWeights(
        List<InputWeight> weights,
        IReadOnlyList<InputKind> activeKinds
    )
    {
        var byKind = weights.GroupBy(w => w.Kind).ToDictionary(g => g.Key, g => g.First().Weight);
        return
        [
            .. activeKinds.Select(kind => new InputWeight(
                kind,
                byKind.TryGetValue(kind, out var w) ? w : 0m
            )),
        ];
    }

    private static decimal GetWeight(List<InputWeight> weights, InputKind kind)
    {
        return weights.FirstOrDefault(w => w.Kind == kind)?.Weight ?? 0m;
    }
}
