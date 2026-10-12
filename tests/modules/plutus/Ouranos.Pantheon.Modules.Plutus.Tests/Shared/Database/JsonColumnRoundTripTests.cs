using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Database;

public sealed class JsonColumnRoundTripTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory =
        DbContextExtensions.MockFactory<PlutusDbContext>();

    [Fact]
    public async Task Strategy_WhenFlipConfigurationSaved_ShouldReloadEqualConfiguration()
    {
        // Arrange
        var configuration = new FlipConfiguration(
            CheckInHours: 4,
            MaxOpenOrders: 6,
            LookbackHours: 48,
            BidOffset: 0.1m,
            AskOffset: -0.05m,
            MinMargin: 0.02m,
            MaxHoldHours: 24,
            MaxPositionPercent: 0.2m,
            MaxPriceMove: 0m,
            MinPrice: 250m
        );

        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Flip",
            null,
            configuration
        );

        // Act
        await SaveAsync(strategy);
        var reloaded = await ReloadStrategyAsync(strategy.Id);

        // Assert
        reloaded.Configuration.ShouldBeOfType<FlipConfiguration>();
        reloaded.Configuration.ShouldBe(configuration);
    }

    [Fact]
    public async Task Strategy_WhenSwingConfigurationSaved_ShouldReloadEquivalentConfiguration()
    {
        // Arrange
        var configuration = new SwingConfiguration(
            [
                new InputWeight(InputKind.SignalRsi, 1.5m),
                new InputWeight(InputKind.SignalVolumeAnomaly, -0.25m),
            ],
            BuyThreshold: 0.1m,
            SellThreshold: -0.2m,
            MaxPositions: 4,
            MaxPositionPercent: 0.3m,
            HoldPeriodDays: 14
        );

        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Swing",
            null,
            configuration
        );

        // Act
        await SaveAsync(strategy);
        var reloaded = await ReloadStrategyAsync(strategy.Id);

        // Assert
        var swing = reloaded.Configuration.ShouldBeOfType<SwingConfiguration>();
        swing.InputWeights.ShouldBe(configuration.InputWeights);
        (swing with { InputWeights = configuration.InputWeights }).ShouldBe(configuration);
    }

    [Fact]
    public async Task Strategy_WhenConfigurationReplaced_ShouldPersistNewConfiguration()
    {
        // Arrange
        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        await SaveAsync(strategy);
        var replacement = new FlipConfiguration(MaxOpenOrders: 2);

        // Act
        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var tracked = await dbContext.Strategies.SingleAsync(s => s.Id == strategy.Id);
            tracked.Update("Test", null, replacement);
            await dbContext.SaveChangesAsync();
        }

        // Assert
        (await ReloadStrategyAsync(strategy.Id)).Configuration.ShouldBe(replacement);
    }

    [Fact]
    public async Task Backtest_WhenRunSettingsAndOptimizedConfigurationSaved_ShouldReloadEqual()
    {
        // Arrange
        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            new FlipConfiguration()
        );

        var runSettings = new BacktestRunSettings(
            0.15m,
            0.08m,
            new OptimizationSettings(12, 24, 0.7, 0.3, 6)
        );

        var backtest = Backtest.Create(
            strategy.Id,
            strategy.MarketId,
            Start,
            Start.AddDays(30),
            5000m,
            runSettings,
            strategy
        );

        var optimized = new FlipConfiguration(MaxOpenOrders: 7, MinMargin: 0.05m);
        backtest.MarkRunning();

        backtest.Complete(
            new BacktestResults
            {
                TotalReturn = 10m,
                OptimizedConfiguration = optimized,
                OutSampleResults = new BacktestResults { TotalReturn = 5m },
            }
        );

        // Act
        await SaveAsync(strategy, backtest);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Assert
        var reloaded = await dbContext
            .Backtests.AsNoTracking()
            .SingleAsync(b => b.Id == backtest.Id);

        reloaded.RunSettings.ShouldBe(runSettings);
        reloaded.Kind.ShouldBe(BacktestKind.Optimization);
        reloaded.Results.ShouldNotBeNull();
        reloaded.Results.OptimizedConfiguration.ShouldBe(optimized);
        reloaded.Results.OutSampleResults.ShouldNotBeNull();
        reloaded.Results.OutSampleResults.TotalReturn.ShouldBe(5m);
    }

    [Fact]
    public async Task Backtest_WhenNoOptimizedConfiguration_ShouldReloadNull()
    {
        // Arrange
        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            new FlipConfiguration()
        );

        var backtest = Backtest.Create(
            strategy.Id,
            strategy.MarketId,
            Start,
            Start.AddDays(30),
            5000m,
            new BacktestRunSettings(),
            strategy
        );

        backtest.MarkRunning();
        backtest.Complete(new BacktestResults { TotalReturn = 1m });

        // Act
        await SaveAsync(strategy, backtest);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Assert
        var reloaded = await dbContext
            .Backtests.AsNoTracking()
            .SingleAsync(b => b.Id == backtest.Id);

        reloaded.RunSettings.ShouldBe(new BacktestRunSettings());
        reloaded.Results.ShouldNotBeNull();
        reloaded.Results.OptimizedConfiguration.ShouldBeNull();
    }

    private async Task SaveAsync(Strategy strategy, Backtest? backtest = null)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.SeedData(strategy);

        if (backtest is not null)
        {
            await dbContext.SeedData(backtest);
        }
    }

    private async Task<Strategy> ReloadStrategyAsync(Id<Strategy> strategyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Strategies.AsNoTracking().SingleAsync(s => s.Id == strategyId);
    }
}
