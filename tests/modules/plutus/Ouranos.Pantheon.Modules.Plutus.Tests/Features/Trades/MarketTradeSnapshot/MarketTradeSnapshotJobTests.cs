using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Trades.MarketSnapshot;

public sealed class MarketTradeSnapshotJobTests
{
    private readonly ILogger<MarketTradeSnapshotJob> _logger =
        Substitute.For<ILogger<MarketTradeSnapshotJob>>();

    private readonly PlutusDbContext _dbContext;
    private readonly TickerFunctionContext _context = new();

    public MarketTradeSnapshotJobTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
    }

    private MarketTradeSnapshotJob CreateJob() =>
        new(_logger, _dbContext, Options.Create(new MarketTradeSnapshotOptions(BatchSize: 500)));

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

}
