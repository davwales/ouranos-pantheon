using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;

/// <summary>
///     Chromosome for the signals-only strategy model. Genes encode the three common
///     <see cref="TradingConfiguration" /> fields, all seven <see cref="InputKind" />
///     weights (in enum order, zero-filled), and the buy/sell thresholds. Mutation is
///     feature-grouped: a single mutation roll picks one of {one common field, one
///     input weight, one threshold} and jitters only that, which keeps the search
///     local and complements the L1 regularization in the fitness function by letting
///     the GA explore dropping individual inputs.
/// </summary>
public sealed class StrategyChromosome(
    TradingConfiguration configuration,
    List<InputWeight> inputWeights,
    InputThresholds thresholds
) : IChromosome<double>
{
    private const int InputWeightCount = 7;
    private const int ThresholdCount = 2;

    public TradingConfiguration Configuration { get; private set; } = configuration;

    public List<InputWeight> InputWeights { get; private set; } = NormalizeWeights(inputWeights);

    public InputThresholds Thresholds { get; private set; } = thresholds;

    public double[] Genes => ToGenes();

    public static StrategyChromosome CreateRandom()
    {
        var random = Random.Shared;
        var configuration = new TradingConfiguration
        {
            MaxPositions = random.Next(1, 20),
            MaxPositionPercent = 0.05m + (decimal)random.NextDouble() * 0.45m,
            HoldPeriodDays = random.Next(1, 30),
        };

        var weights = new List<InputWeight>(InputWeightCount);
        foreach (InputKind kind in Enum.GetValues<InputKind>())
        {
            weights.Add(new InputWeight(kind, (decimal)random.NextDouble() * 2m));
        }

        var thresholds = new InputThresholds
        {
            BuyThreshold = (decimal)random.NextDouble() * 0.5m,
            SellThreshold = -(decimal)random.NextDouble() * 0.5m,
        };

        return new StrategyChromosome(configuration, weights, thresholds);
    }

    public BacktestParameters ApplyConfigOverrides(BacktestParameters parameters)
    {
        return parameters with
        {
            InputWeightsOverride = InputWeights,
            ThresholdsOverride = Thresholds,
        };
    }

    public void Mutate(double mutationRate)
    {
        var random = Random.Shared;
        if (random.NextDouble() >= mutationRate)
        {
            return;
        }

        var group = random.Next(3);
        switch (group)
        {
            case 0:
                MutateCommonFields(random, mutationRate);
                break;
            case 1:
                MutateOneWeight(random);
                break;
            case 2:
                MutateOneThreshold(random);
                break;
        }
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
        var childConfig = CrossoverCommonFields(
            Configuration,
            otherChromosome.Configuration,
            random
        );

        var childWeights = new List<InputWeight>(InputWeightCount);
        foreach (InputKind kind in Enum.GetValues<InputKind>())
        {
            var parent1Weight = GetWeight(InputWeights, kind);
            var parent2Weight = GetWeight(otherChromosome.InputWeights, kind);
            var childWeight = random.NextDouble() < 0.5 ? parent1Weight : parent2Weight;
            childWeights.Add(new InputWeight(kind, childWeight));
        }

        var childThresholds = new InputThresholds
        {
            BuyThreshold =
                random.NextDouble() < 0.5
                    ? Thresholds.BuyThreshold
                    : otherChromosome.Thresholds.BuyThreshold,
            SellThreshold =
                random.NextDouble() < 0.5
                    ? Thresholds.SellThreshold
                    : otherChromosome.Thresholds.SellThreshold,
        };

        return new StrategyChromosome(childConfig, childWeights, childThresholds);
    }

    private void AddStrategySpecificGenes(List<double> genes)
    {
        foreach (InputKind kind in Enum.GetValues<InputKind>())
        {
            genes.Add((double)GetWeight(InputWeights, kind));
        }

        genes.Add((double)(Thresholds.BuyThreshold ?? 0m));
        genes.Add((double)(Thresholds.SellThreshold ?? 0m));
    }

    private double[] ToGenes()
    {
        var genes = new List<double>();

        AddGeneIfHasValue(genes, Configuration.MaxPositions);
        AddGeneIfHasValue(genes, Configuration.MaxPositionPercent, v => (double)v);
        AddGeneIfHasValue(genes, Configuration.HoldPeriodDays, v => v);
        AddStrategySpecificGenes(genes);

        return [.. genes];
    }

    private void MutateCommonFields(Random random, double mutationRate)
    {
        if (random.NextDouble() >= mutationRate)
        {
            return;
        }

        var field = random.Next(3);

        if (field == 0 && Configuration.MaxPositions.HasValue)
        {
            Configuration = Configuration with
            {
                MaxPositions = Math.Max(1, Configuration.MaxPositions.Value + random.Next(-3, 4)),
            };
        }
        else if (field == 1 && Configuration.MaxPositionPercent.HasValue)
        {
            Configuration = Configuration with
            {
                MaxPositionPercent = Math.Clamp(
                    Configuration.MaxPositionPercent.Value
                        + (decimal)(random.NextDouble() - 0.5) * 0.1m,
                    0.05m,
                    0.5m
                ),
            };
        }
        else if (field == 2 && Configuration.HoldPeriodDays.HasValue)
        {
            Configuration = Configuration with
            {
                HoldPeriodDays = Math.Max(
                    1,
                    Configuration.HoldPeriodDays.Value + random.Next(-5, 6)
                ),
            };
        }
    }

    private static TradingConfiguration CrossoverCommonFields(
        TradingConfiguration parent1,
        TradingConfiguration parent2,
        Random random
    )
    {
        return new TradingConfiguration
        {
            MaxPositions = random.NextDouble() < 0.5 ? parent1.MaxPositions : parent2.MaxPositions,
            MaxPositionPercent =
                random.NextDouble() < 0.5 ? parent1.MaxPositionPercent : parent2.MaxPositionPercent,
            HoldPeriodDays =
                random.NextDouble() < 0.5 ? parent1.HoldPeriodDays : parent2.HoldPeriodDays,
        };
    }

    private static decimal MutateWeight(decimal current, Random random, double mutationRate)
    {
        return Math.Clamp(
            current
                + (
                    random.NextDouble() < mutationRate
                        ? (decimal)(random.NextDouble() - 0.5) * 0.4m
                        : 0m
                ),
            0m,
            3m
        );
    }

    private static decimal MutateThreshold(
        decimal? current,
        Random random,
        double mutationRate,
        decimal min,
        decimal max
    )
    {
        var value = current ?? 0m;
        return Math.Clamp(
            value
                + (
                    random.NextDouble() < mutationRate
                        ? (decimal)(random.NextDouble() - 0.5) * 0.2m
                        : 0m
                ),
            min,
            max
        );
    }

    private void MutateOneWeight(Random random)
    {
        var kinds = Enum.GetValues<InputKind>();
        var kind = kinds[random.Next(kinds.Length)];
        var current = GetWeight(InputWeights, kind);
        var mutated = MutateWeight(current, random, mutationRate: 1.0);
        InputWeights = WithWeight(InputWeights, kind, mutated);
    }

    private void MutateOneThreshold(Random random)
    {
        var field = random.Next(ThresholdCount);
        switch (field)
        {
            case 0:
                Thresholds = Thresholds with
                {
                    BuyThreshold = MutateThreshold(
                        Thresholds.BuyThreshold,
                        random,
                        mutationRate: 1.0,
                        min: 0m,
                        max: 0.5m
                    ),
                };
                break;
            case 1:
                Thresholds = Thresholds with
                {
                    SellThreshold = MutateThreshold(
                        Thresholds.SellThreshold,
                        random,
                        mutationRate: 1.0,
                        min: -0.5m,
                        max: 0m
                    ),
                };
                break;
        }
    }

    /// <summary>
    ///     Ensures the weight list has exactly one entry per <see cref="InputKind" /> in
    ///     enum order, zero-filling any missing kinds. Required for deterministic gene
    ///     vectors and crossover.
    /// </summary>
    private static List<InputWeight> NormalizeWeights(List<InputWeight> weights)
    {
        var byKind = weights.GroupBy(w => w.Kind).ToDictionary(g => g.Key, g => g.First().Weight);

        var result = new List<InputWeight>(InputWeightCount);
        foreach (InputKind kind in Enum.GetValues<InputKind>())
        {
            result.Add(new InputWeight(kind, byKind.TryGetValue(kind, out var w) ? w : 0m));
        }

        return result;
    }

    private static decimal GetWeight(List<InputWeight> weights, InputKind kind)
    {
        return weights.FirstOrDefault(w => w.Kind == kind)?.Weight ?? 0m;
    }

    private static List<InputWeight> WithWeight(
        List<InputWeight> weights,
        InputKind kind,
        decimal newWeight
    )
    {
        return [.. weights.Select(w => w.Kind == kind ? w with { Weight = newWeight } : w)];
    }

    private static void AddGeneIfHasValue<T>(List<double> genes, T? value, Func<T, double> convert)
        where T : struct
    {
        if (value.HasValue)
        {
            genes.Add(convert(value.Value));
        }
    }

    private static void AddGeneIfHasValue(List<double> genes, int? value)
    {
        if (value.HasValue)
        {
            genes.Add(value.Value);
        }
    }
}
