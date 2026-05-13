using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.MarketTradeSnapshot;

public sealed class MarketTradeSnapshotJobTests
{
    private readonly ILogger<MarketTradeSnapshotJob> _logger = Substitute.For<
        ILogger<MarketTradeSnapshotJob>
    >();

    private readonly PlutusDbContext _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
    private readonly TickerFunctionContext _context = new();

    private MarketTradeSnapshotJob CreateJob()
    {
        return new(
            _logger,
            _dbContext,
            Options.Create(new MarketTradeSnapshotOptions(BatchSize: 500))
        );
    }

    [Fact]
    public async Task ExecuteFifteenMinutes_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteFifteenMinutes(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneHour_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteOneHour(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteFourHours_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteFourHours(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneDay_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteOneDay(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAllTime_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteAllTime(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneWeek_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteOneWeek(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneMonth_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteOneMonth(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteSixMonths_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteSixMonths(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteOneYear_WhenNoTrades_ShouldCreateNoSnapshots()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.ExecuteOneYear(_context, CancellationToken.None);

        // Assert
        _dbContext.MarketTradeSnapshots.Count().ShouldBe(0);
    }
}
