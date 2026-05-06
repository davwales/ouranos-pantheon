using Ouranos.Pantheon.Modules.Shared.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;

public sealed class StrategyConfigurationChromosome(StrategyType strategyType, StrategyConfiguration configuration)
    : IChromosome<double>
{
    public StrategyConfiguration Configuration { get; private set; } = configuration;

    public double[] Genes => ToGenes();

    public StrategyConfigurationChromosome(StrategyType strategyType) : this(
        strategyType,
        CreateRandomConfiguration(strategyType)
    )
    {
    }

    public void Mutate(double mutationRate)
    {
        var random = Random.Shared;

        MutateCommonFields(random, mutationRate);
        MutateStrategySpecificFields(random, mutationRate);
    }

    public IChromosome<double> Crossover(IChromosome<double> other)
    {
        if (other is not StrategyConfigurationChromosome otherChromosome)
        {
            throw new InvalidOperationException(
                $"Crossover partner must be a {nameof(StrategyConfigurationChromosome)}."
            );
        }

        var random = Random.Shared;
        var parent1 = Configuration;
        var parent2 = otherChromosome.Configuration;

        var child = CrossoverCommonFields(parent1, parent2, random);
        child = CrossoverStrategySpecificFields(child, parent1, parent2, random);

        return new StrategyConfigurationChromosome(strategyType, child);
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
                MaxPositions = Math.Max(1, Configuration.MaxPositions.Value + random.Next(-3, 4))
            };
        }
        else if (field == 1 && Configuration.MaxPositionPercent.HasValue)
        {
            Configuration = Configuration with
            {
                MaxPositionPercent = Math.Clamp(
                    Configuration.MaxPositionPercent.Value + (decimal)(random.NextDouble() - 0.5) * 0.1m,
                    0.05m,
                    0.5m
                )
            };
        }
        else if (field == 2 && Configuration.HoldPeriodDays.HasValue)
        {
            Configuration = Configuration with
            {
                HoldPeriodDays = Math.Max(1, Configuration.HoldPeriodDays.Value + random.Next(-5, 6))
            };
        }
    }

    private void MutateStrategySpecificFields(Random random, double mutationRate)
    {
        if (random.NextDouble() >= mutationRate)
        {
            return;
        }

        switch (strategyType)
        {
            case StrategyType.SignalWeighted:
                MutateSignalWeightedFields(random, mutationRate);
                break;
            case StrategyType.ForecastMomentum:
                MutateForecastMomentumFields(random, mutationRate);
                break;
            case StrategyType.MeanReversion:
                MutateMeanReversionFields(random, mutationRate);
                break;
            case StrategyType.RecipeArbitrage:
                MutateRecipeArbitrageFields(random, mutationRate);
                break;
        }
    }

    private void MutateSignalWeightedFields(Random random, double mutationRate)
    {
        if (random.NextDouble() < mutationRate && Configuration.BuyThreshold.HasValue)
        {
            Configuration = Configuration with
            {
                BuyThreshold = Math.Clamp(
                    Configuration.BuyThreshold.Value + (decimal)(random.NextDouble() - 0.5) * 0.2m,
                    0.01m,
                    2m
                )
            };
        }

        if (random.NextDouble() < mutationRate && Configuration.SellThreshold.HasValue)
        {
            Configuration = Configuration with
            {
                SellThreshold = -Math.Abs(
                    Math.Clamp(
                        Configuration.SellThreshold.Value + (decimal)(random.NextDouble() - 0.5) * 0.2m,
                        -2m,
                        -0.01m
                    )
                )
            };
        }

        if (random.NextDouble() >= mutationRate || Configuration.SignalWeights is null)
        {
            return;
        }

        var mutatedWeights = Configuration.SignalWeights
            .Select(w => random.NextDouble() < mutationRate
                ? w with { Weight = Math.Clamp(w.Weight + (decimal)(random.NextDouble() - 0.5) * 0.4m, 0m, 3m) }
                : w
            )
            .ToList();

        Configuration = Configuration with { SignalWeights = mutatedWeights };
    }

    private void MutateForecastMomentumFields(Random random, double mutationRate)
    {
        if (random.NextDouble() < mutationRate && Configuration.ForecastMovementThreshold.HasValue)
        {
            Configuration = Configuration with
            {
                ForecastMovementThreshold = Math.Clamp(
                    Configuration.ForecastMovementThreshold.Value + (decimal)(random.NextDouble() - 0.5) * 0.02m,
                    0.005m,
                    0.2m
                )
            };
        }

        if (random.NextDouble() < mutationRate && Configuration.ForecastHorizonDays.HasValue)
        {
            Configuration = Configuration with
            {
                ForecastHorizonDays = Math.Clamp(
                    Configuration.ForecastHorizonDays.Value + random.Next(-2, 3),
                    1,
                    30
                )
            };
        }
    }

    private void MutateMeanReversionFields(Random random, double mutationRate)
    {
        if (random.NextDouble() < mutationRate && Configuration.DeviationMultiplier.HasValue)
        {
            Configuration = Configuration with
            {
                DeviationMultiplier = Math.Clamp(
                    Configuration.DeviationMultiplier.Value + (decimal)(random.NextDouble() - 0.5) * 0.5m,
                    0.5m,
                    4m
                )
            };
        }

        if (random.NextDouble() < mutationRate && Configuration.MeanTimeFrameValue.HasValue)
        {
            Configuration = Configuration with
            {
                MeanTimeFrameValue = Math.Clamp(
                    Configuration.MeanTimeFrameValue.Value + random.Next(-5, 6),
                    5,
                    30
                )
            };
        }
    }

    private void MutateRecipeArbitrageFields(Random random, double mutationRate)
    {
        if (random.NextDouble() < mutationRate && Configuration.MinMarginPercent.HasValue)
        {
            Configuration = Configuration with
            {
                MinMarginPercent = Math.Clamp(
                    Configuration.MinMarginPercent.Value + (decimal)(random.NextDouble() - 0.5) * 0.04m,
                    0.005m,
                    0.3m
                )
            };
        }
    }

    private static StrategyConfiguration CrossoverCommonFields(
        StrategyConfiguration parent1,
        StrategyConfiguration parent2,
        Random random
    )
    {
        return new StrategyConfiguration
        {
            MaxPositions = random.NextDouble() < 0.5 ? parent1.MaxPositions : parent2.MaxPositions,
            MaxPositionPercent =
                random.NextDouble() < 0.5 ? parent1.MaxPositionPercent : parent2.MaxPositionPercent,
            HoldPeriodDays = random.NextDouble() < 0.5 ? parent1.HoldPeriodDays : parent2.HoldPeriodDays,
        };
    }

    private StrategyConfiguration CrossoverStrategySpecificFields(
        StrategyConfiguration child,
        StrategyConfiguration parent1,
        StrategyConfiguration parent2,
        Random random
    )
    {
        return strategyType switch
        {
            StrategyType.SignalWeighted => child with
            {
                BuyThreshold = random.NextDouble() < 0.5 ? parent1.BuyThreshold : parent2.BuyThreshold,
                SellThreshold = random.NextDouble() < 0.5 ? parent1.SellThreshold : parent2.SellThreshold,
                SignalWeights = CrossoverSignalWeights(parent1.SignalWeights, parent2.SignalWeights, random),
                ForecastMovementThreshold = random.NextDouble() < 0.5
                    ? parent1.ForecastMovementThreshold
                    : parent2.ForecastMovementThreshold,
                ForecastHorizonDays = random.NextDouble() < 0.5
                    ? parent1.ForecastHorizonDays
                    : parent2.ForecastHorizonDays,
            },
            StrategyType.ForecastMomentum => child with
            {
                ForecastMovementThreshold = random.NextDouble() < 0.5
                    ? parent1.ForecastMovementThreshold
                    : parent2.ForecastMovementThreshold,
                ForecastHorizonDays = random.NextDouble() < 0.5
                    ? parent1.ForecastHorizonDays
                    : parent2.ForecastHorizonDays,
            },
            StrategyType.MeanReversion => child with
            {
                DeviationMultiplier = random.NextDouble() < 0.5
                    ? parent1.DeviationMultiplier
                    : parent2.DeviationMultiplier,
                MeanTimeFrameValue = random.NextDouble() < 0.5
                    ? parent1.MeanTimeFrameValue
                    : parent2.MeanTimeFrameValue,
            },
            StrategyType.RecipeArbitrage => child with
            {
                MinMarginPercent = random.NextDouble() < 0.5
                    ? parent1.MinMarginPercent
                    : parent2.MinMarginPercent,
            },
            _ => child
        };
    }

    private static List<SignalWeight>? CrossoverSignalWeights(
        List<SignalWeight>? parent1Weights,
        List<SignalWeight>? parent2Weights,
        Random random
    )
    {
        if (parent1Weights is null || parent2Weights is null)
        {
            return parent1Weights ?? parent2Weights;
        }

        return
        [
            .. parent1Weights
                .Select(p1 =>
                    {
                        var matchingP2 = parent2Weights.FirstOrDefault(p2 => p2.Type == p1.Type);
                        var weight = matchingP2 is not null && random.NextDouble() < 0.5
                            ? matchingP2.Weight
                            : p1.Weight;
                        return new SignalWeight(p1.Type, weight);
                    }
                )
        ];
    }

    private double[] ToGenes()
    {
        var genes = new List<double>();

        AddGeneIfHasValue(genes, Configuration.MaxPositions);
        AddGeneIfHasValue(genes, Configuration.MaxPositionPercent, v => (double)v);
        AddGeneIfHasValue(genes, Configuration.HoldPeriodDays, v => v);

        if (strategyType == StrategyType.SignalWeighted)
        {
            AddGeneIfHasValue(genes, Configuration.BuyThreshold, v => (double)v);
            AddGeneIfHasValue(genes, Configuration.SellThreshold, v => (double)v);
            AddSignalWeightGenes(genes);
        }

        if (strategyType is StrategyType.SignalWeighted or StrategyType.ForecastMomentum)
        {
            AddGeneIfHasValue(genes, Configuration.ForecastMovementThreshold, v => (double)v);
            AddGeneIfHasValue(genes, Configuration.ForecastHorizonDays, v => v);
        }

        if (strategyType == StrategyType.MeanReversion)
        {
            AddGeneIfHasValue(genes, Configuration.DeviationMultiplier, v => (double)v);
            AddGeneIfHasValue(genes, Configuration.MeanTimeFrameValue, v => v);
        }

        if (strategyType == StrategyType.RecipeArbitrage)
        {
            AddGeneIfHasValue(genes, Configuration.MinMarginPercent, v => (double)v);
        }

        return [.. genes];
    }

    private static void AddGeneIfHasValue<T>(List<double> genes, T? value, Func<T, double> convert) where T : struct
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

    private void AddSignalWeightGenes(List<double> genes)
    {
        if (Configuration.SignalWeights is null)
        {
            return;
        }

        foreach (var weight in Configuration.SignalWeights)
        {
            genes.Add((double)weight.Weight);
        }
    }

    internal static StrategyConfiguration CreateRandomConfiguration(StrategyType type)
    {
        var random = Random.Shared;

        return type switch
        {
            StrategyType.SignalWeighted => new StrategyConfiguration
            {
                SignalWeights =
                [
                    .. Enum.GetValues<SignalType>().Select(st => new SignalWeight(
                            st,
                            (decimal)random.NextDouble() * 2
                        )
                    )
                ],
                BuyThreshold = (decimal)random.NextDouble(),
                SellThreshold = -(decimal)random.NextDouble(),
                MaxPositions = random.Next(1, 20),
                MaxPositionPercent = 0.05m + (decimal)random.NextDouble() * 0.45m,
                HoldPeriodDays = random.Next(1, 30)
            },
            StrategyType.ForecastMomentum => new StrategyConfiguration
            {
                ForecastMovementThreshold = 0.01m + (decimal)random.NextDouble() * 0.1m,
                ForecastHorizonDays = random.Next(1, 14),
                MaxPositions = random.Next(1, 20),
                MaxPositionPercent = 0.05m + (decimal)random.NextDouble() * 0.45m,
                HoldPeriodDays = random.Next(1, 30)
            },
            StrategyType.MeanReversion => new StrategyConfiguration
            {
                DeviationMultiplier = 0.5m + (decimal)random.NextDouble() * 3m,
                MeanTimeFrameValue = random.Next(5, 26),
                MaxPositions = random.Next(1, 20),
                MaxPositionPercent = 0.05m + (decimal)random.NextDouble() * 0.45m,
                HoldPeriodDays = random.Next(1, 30)
            },
            StrategyType.RecipeArbitrage => new StrategyConfiguration
            {
                MinMarginPercent = 0.01m + (decimal)random.NextDouble() * 0.2m,
                MaxPositions = random.Next(1, 20),
                MaxPositionPercent = 0.05m + (decimal)random.NextDouble() * 0.45m,
                HoldPeriodDays = random.Next(1, 30)
            },
            _ => new StrategyConfiguration
            {
                MaxPositions = random.Next(1, 20),
                MaxPositionPercent = 0.05m + (decimal)random.NextDouble() * 0.45m,
                HoldPeriodDays = random.Next(1, 30)
            }
        };
    }
}