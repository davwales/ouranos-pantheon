using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetRecommendations;

public sealed class GetRecommendationsHandlerTests
{
    private readonly PlutusDbContext _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
    private readonly FakeStrategyEngine _swingEngine;
    private readonly FakeStrategyEngine _flipEngine;
    private readonly GetRecommendationsHandler _handler;

    public GetRecommendationsHandlerTests()
    {
        _swingEngine = new FakeStrategyEngine(StrategyMode.Swing)
        {
            Recommendations = [Recommendation("swing")],
        };

        _flipEngine = new FakeStrategyEngine(StrategyMode.Flip)
        {
            Recommendations = [Recommendation("flip-1"), Recommendation("flip-2")],
        };

        _handler = new GetRecommendationsHandler(
            Substitute.For<ILogger<GetRecommendationsHandler>>(),
            _dbContext,
            new StrategyEngines([_swingEngine, _flipEngine])
        );
    }

    private static StrategyRecommendation Recommendation(string symbolId)
    {
        return new StrategyRecommendation(
            symbolId,
            symbolId,
            null,
            1m,
            100m,
            10m,
            10m,
            1000m,
            PositionLimit.Budget,
            "Test"
        );
    }

    private async Task<Strategy> SeedStrategyAsync(StrategyConfiguration configuration)
    {
        var strategy = Strategy.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test",
            null,
            configuration
        );
        await _dbContext.SeedData(strategy);
        return strategy;
    }

    private static Backtest CreateBacktest(
        Strategy strategy,
        decimal participationRate,
        BacktestStatus status
    )
    {
        var backtest = Backtest.Create(
            strategy.Id,
            strategy.MarketId,
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow,
            1000m,
            new BacktestRunSettings(participationRate),
            strategy
        );
        backtest.MarkRunning();

        if (status == BacktestStatus.Completed)
        {
            backtest.Complete(new BacktestResults());
        }
        else
        {
            backtest.Fail("Test");
        }

        return backtest;
    }

    [Fact]
    public async Task Handle_WhenFlipStrategy_ShouldDelegateToFlipEngine()
    {
        // Arrange
        var configuration = new FlipConfiguration(MaxOpenOrders: 5);
        var strategy = await SeedStrategyAsync(configuration);

        // Act
        var result = await _handler.Handle(
            new GetRecommendationsInput(strategy.Id, strategy.MarketId, 5000m),
            CancellationToken.None
        );

        // Assert
        result.Recommendations.Select(r => r.SymbolId).ShouldBe(["flip-1", "flip-2"]);

        var (MarketId, Configuration, Budget, ParticipationRate) =
            _flipEngine.RecommendCalls.ShouldHaveSingleItem();

        MarketId.ShouldBe(strategy.MarketId);
        Configuration.ShouldBe(configuration);
        Budget.ShouldBe(5000m);
        ParticipationRate.ShouldBe(BacktestDataOptions.DefaultVolumeParticipationRate);
        result.VolumeParticipationRate.ShouldBe(BacktestDataOptions.DefaultVolumeParticipationRate);
        _swingEngine.RecommendCalls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSwingStrategy_ShouldDelegateToSwingEngine()
    {
        // Arrange
        var strategy = await SeedStrategyAsync(StrategyTestFactory.Swing());

        // Act
        var result = await _handler.Handle(
            new GetRecommendationsInput(strategy.Id, strategy.MarketId, 1000m),
            CancellationToken.None
        );

        // Assert
        result.Recommendations.ShouldHaveSingleItem().SymbolId.ShouldBe("swing");
        _swingEngine.RecommendCalls.ShouldHaveSingleItem().Budget.ShouldBe(1000m);
        _flipEngine.RecommendCalls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenStrategyHasCompletedBacktests_ShouldUseTheLatestParticipationRate()
    {
        // Arrange
        var strategy = await SeedStrategyAsync(new FlipConfiguration());
        await _dbContext.SeedData(
            CreateBacktest(strategy, 0.05m, BacktestStatus.Completed),
            CreateBacktest(strategy, 0.2m, BacktestStatus.Completed),
            CreateBacktest(strategy, 0.5m, BacktestStatus.Failed)
        );

        // Act
        var result = await _handler.Handle(
            new GetRecommendationsInput(strategy.Id, strategy.MarketId, 1000m),
            CancellationToken.None
        );

        // Assert
        _flipEngine.RecommendCalls.ShouldHaveSingleItem().ParticipationRate.ShouldBe(0.2m);
        result.VolumeParticipationRate.ShouldBe(0.2m);
    }

    [Fact]
    public async Task Handle_WhenParticipationRateGiven_ShouldOverrideTheBacktestRate()
    {
        // Arrange
        var strategy = await SeedStrategyAsync(new FlipConfiguration());
        await _dbContext.SeedData(CreateBacktest(strategy, 0.2m, BacktestStatus.Completed));

        // Act
        var result = await _handler.Handle(
            new GetRecommendationsInput(strategy.Id, strategy.MarketId, 1000m, 0.03m),
            CancellationToken.None
        );

        // Assert
        _flipEngine.RecommendCalls.ShouldHaveSingleItem().ParticipationRate.ShouldBe(0.03m);
        result.VolumeParticipationRate.ShouldBe(0.03m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(1.5)]
    public async Task Handle_WhenParticipationRateOutOfRange_ShouldThrowArgumentException(
        decimal participationRate
    )
    {
        // Arrange
        var strategy = await SeedStrategyAsync(new FlipConfiguration());
        var input = new GetRecommendationsInput(
            strategy.Id,
            strategy.MarketId,
            1000m,
            participationRate
        );

        // Act
        var handle = () => _handler.Handle(input, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<ArgumentException>();
        _flipEngine.RecommendCalls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenStrategyNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var input = new GetRecommendationsInput(
            new Id<Strategy>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            1000m
        );

        // Act
        var handle = () => _handler.Handle(input, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenStrategyBelongsToAnotherMarket_ShouldThrowArgumentException()
    {
        // Arrange
        var strategy = await SeedStrategyAsync(new FlipConfiguration());

        var input = new GetRecommendationsInput(
            strategy.Id,
            new Id<Market>(Guid.NewGuid().ToString()),
            1000m
        );

        // Act
        var handle = () => _handler.Handle(input, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<ArgumentException>();
        _flipEngine.RecommendCalls.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Handle_WhenBudgetNotPositive_ShouldThrowArgumentException(decimal budget)
    {
        // Arrange
        var strategy = await SeedStrategyAsync(new FlipConfiguration());
        var input = new GetRecommendationsInput(strategy.Id, strategy.MarketId, budget);

        // Act
        var handle = () => _handler.Handle(input, CancellationToken.None);

        // Assert
        await handle.ShouldThrowAsync<ArgumentException>();
        _flipEngine.RecommendCalls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var input = new GetRecommendationsInput(
            new Id<Strategy>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            1000m
        );

        // Act
        var handle = () => _handler.Handle(input, new CancellationToken(true));

        // Assert
        await handle.ShouldThrowAsync<OperationCanceledException>();
    }
}
