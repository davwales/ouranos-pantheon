using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization.Chromosomes;
using Ouranos.Pantheon.Modules.Shared.Contract.Algorithms.Genetic;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Optimization.Chromosomes;

public sealed class StrategyChromosomeTests
{
    private static readonly InputKind[] ActiveKinds =
    [
        InputKind.SignalRsi,
        InputKind.SignalBollingerBands,
        InputKind.SignalTrendMomentum,
    ];

    private static StrategyChromosome Chromosome()
    {
        return new StrategyChromosome(
            new TradingConfiguration
            {
                MaxPositions = 5,
                MaxPositionPercent = 0.25m,
                HoldPeriodDays = 10,
            },
            [
                new(InputKind.SignalRsi, 1m),
                new(InputKind.SignalBollingerBands, 2m),
                new(InputKind.SignalTrendMomentum, -0.5m),
            ],
            new InputThresholds(0.3m, -0.1m)
        );
    }

    [Fact]
    public void Genes_ShouldContainCommonFieldsThenActiveWeightsInEnumOrderThenThresholds()
    {
        // Arrange
        var chromosome = Chromosome();

        // Act
        var genes = chromosome.Genes;

        // Assert
        genes.ShouldBe([5, 0.25, 10, -0.5, 2, 1, 0.3, -0.1]);
    }

    [Fact]
    public void InputWeights_ShouldOnlyContainTheStrategysOwnKinds()
    {
        // Act
        var chromosome = Chromosome();

        // Assert
        chromosome
            .InputWeights.Select(w => w.Kind)
            .ShouldBe([
                InputKind.SignalTrendMomentum,
                InputKind.SignalBollingerBands,
                InputKind.SignalRsi,
            ]);
    }

    [Fact]
    public void CreateRandom_ShouldStayWithinRangesAndOnlyUseActiveKinds()
    {
        // Act
        var chromosomes = Enumerable
            .Range(0, 200)
            .Select(_ => StrategyChromosome.CreateRandom(ActiveKinds))
            .ToList();

        // Assert
        chromosomes.ShouldAllBe(c => c.InputWeights.Count == ActiveKinds.Length);
        chromosomes.ShouldAllBe(c => c.InputWeights.All(w => ActiveKinds.Contains(w.Kind)));
        chromosomes.ShouldAllBe(c => c.InputWeights.All(w => Math.Abs(w.Weight) <= 2m));
        chromosomes.ShouldAllBe(c =>
            c.Thresholds.BuyThreshold >= StrategyChromosome.MinBuyThreshold
            && c.Thresholds.BuyThreshold <= StrategyChromosome.MaxBuyThreshold
        );
        chromosomes.ShouldAllBe(c =>
            c.Configuration.MaxPositionPercent >= StrategyChromosome.MinPositionPercent
            && c.Configuration.MaxPositionPercent <= StrategyChromosome.MaxPositionPercent
        );
        chromosomes.ShouldContain(c => c.InputWeights.Any(w => w.Weight < 0m));
    }

    [Fact]
    public void Mutate_WhenRateIsOne_ShouldChangeGenesButStayWithinRanges()
    {
        // Arrange
        var chromosomes = Enumerable.Range(0, 200).Select(_ => Chromosome()).ToList();

        // Act
        foreach (var chromosome in chromosomes)
        {
            chromosome.Mutate(1.0);
        }

        // Assert
        chromosomes.ShouldContain(c => !c.Genes.SequenceEqual(Chromosome().Genes));
        chromosomes.ShouldAllBe(c =>
            c.InputWeights.All(w => Math.Abs(w.Weight) <= StrategyChromosome.MaxAbsWeight)
        );
        chromosomes.ShouldAllBe(c =>
            c.Thresholds.SellThreshold >= StrategyChromosome.MinSellThreshold
            && c.Thresholds.SellThreshold <= StrategyChromosome.MaxSellThreshold
        );
        chromosomes.ShouldAllBe(c =>
            c.Configuration.HoldPeriodDays >= 1
            && c.Configuration.HoldPeriodDays <= StrategyChromosome.MaxHoldPeriodDays
        );
        chromosomes.ShouldAllBe(c => c.InputWeights.Count == ActiveKinds.Length);
    }

    [Fact]
    public void Crossover_ChildGenesAllComeFromEitherParent()
    {
        // Arrange
        var parent1 = StrategyChromosome.CreateRandom(ActiveKinds);
        var parent2 = StrategyChromosome.CreateRandom(ActiveKinds);

        // Act
        var child = parent1.Crossover(parent2);

        // Assert
        child.ShouldBeOfType<StrategyChromosome>();
        for (var i = 0; i < child.Genes.Length; i++)
        {
            child.Genes[i].ShouldBeOneOf(parent1.Genes[i], parent2.Genes[i]);
        }
    }

    [Fact]
    public void Crossover_WhenPartnerIsNotChromosome_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var chromosome = Chromosome();
        var other = Substitute.For<IChromosome<double>>();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => chromosome.Crossover(other));
    }

    [Fact]
    public void ApplyConfigOverrides_ShouldOverrideConfigurationWeightsAndThresholds()
    {
        // Arrange
        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            new InputThresholds(BuyThreshold: 0.1m)
        );

        var parameters = new BacktestParameters(
            new Id<Market>(Guid.NewGuid().ToString()),
            strategy,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(10),
            10000m
        );

        var chromosome = Chromosome();

        // Act
        var overridden = chromosome.ApplyConfigOverrides(parameters);

        // Assert
        overridden.Configuration.ShouldBe(chromosome.Configuration);
        overridden.InputWeights.ShouldBe(chromosome.InputWeights);
        overridden.Thresholds.ShouldBe(chromosome.Thresholds);
    }
}
