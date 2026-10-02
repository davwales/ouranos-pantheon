using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.Shared;

public sealed class BacktestCancellationTests
{
    private readonly IDbContextFactory<PlutusDbContext> _dbContextFactory;
    private readonly ILogger _logger = Substitute.For<ILogger>();

    public BacktestCancellationTests()
    {
        _dbContextFactory = DbContextExtensions.MockFactory<PlutusDbContext>();
    }

    private static Backtest CreateRunningBacktest(Id<Market> marketId)
    {
        var strategy = Strategy.Create(
            marketId,
            "Test Strategy",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var backtest = Backtest.Create(
            strategy.Id,
            marketId,
            DateTimeOffset.UtcNow.AddDays(-5),
            DateTimeOffset.UtcNow.AddDays(-1),
            10000m,
            strategy
        );
        backtest.MarkRunning();
        return backtest;
    }

    [Fact]
    public async Task Finalize_WhenBacktestCancelledExternally_ShouldNotChangeStatus()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var backtest = CreateRunningBacktest(marketId);

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.SeedData(backtest);
        }

        await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var row = await dbContext.Backtests.SingleAsync(b => b.Id == backtest.Id);
            row.Cancel("Cancelled by user.");
            await dbContext.SaveChangesAsync();
        }

        // Act
        await using var finalizeContext = await _dbContextFactory.CreateDbContextAsync();
        await BacktestCancellation.FinalizeAsync(backtest, finalizeContext, _logger);

        // Assert
        await using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var saved = await verifyContext
            .Backtests.AsNoTracking()
            .SingleAsync(b => b.Id == backtest.Id);

        saved.Status.ShouldBe(BacktestStatus.Cancelled);
    }

    [Fact]
    public async Task Finalize_WhenNoExternalChange_ShouldMarkBacktestFailed()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var backtest = CreateRunningBacktest(marketId);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await dbContext.SeedData(backtest);

        // Act
        await BacktestCancellation.FinalizeAsync(backtest, dbContext, _logger);

        // Assert
        var saved = await dbContext.Backtests.AsNoTracking().SingleAsync(b => b.Id == backtest.Id);

        saved.Status.ShouldBe(BacktestStatus.Failed);
        saved.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Finalize_WhenBacktestNoLongerExists_ShouldNotThrow()
    {
        // Arrange
        var marketId = new Id<Market>(Guid.NewGuid().ToString());
        var backtest = CreateRunningBacktest(marketId);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Act
        var act = async () =>
            await BacktestCancellation.FinalizeAsync(backtest, dbContext, _logger);

        // Assert
        await act.ShouldNotThrowAsync();
    }
}
