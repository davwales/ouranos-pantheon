using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.SymbolSignalCalculate;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.SymbolSignalCalculate;

public sealed class SymbolSignalCalculateJobTests
{
    private readonly ILogger<SymbolSignalCalculateJob> _logger = Substitute.For<
        ILogger<SymbolSignalCalculateJob>
    >();

    private readonly PlutusDbContext _dbContext;
    private readonly TickerFunctionContext _context = new();
    private readonly SignalOptions _signalOptions = new();

    public SymbolSignalCalculateJobTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
    }

    private SymbolSignalCalculateJob CreateJob(IEnumerable<ISignalComputer>? computers = null)
    {
        return new(_logger, _dbContext, Options.Create(_signalOptions), computers ?? []);
    }

    [Fact]
    public async Task Execute_WhenNoSymbols_ShouldCreateNoSignals()
    {
        // Arrange
        var job = CreateJob();

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        _dbContext.Signals.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Execute_WhenSymbolsExistButComputerReturnsNull_ShouldCreateNoSignals()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Market",
            new Taxes(null)
        );
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "ITEM1",
            null,
            "Item One",
            market.Id,
            new AdditionalFields()
        );

        var computer = Substitute.For<ISignalComputer>();
        computer
            .ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<decimal?>(null));

        var job = CreateJob([computer]);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        _dbContext.Signals.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Execute_WhenSymbolsExistAndComputerReturnsValue_ShouldCreateSignals()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Market",
            new Taxes(null)
        );
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "ITEM1",
            null,
            "Item One",
            market.Id,
            new AdditionalFields()
        );

        var computer = Substitute.For<ISignalComputer>();
        computer.Type.Returns(SignalType.TaxAdjustedRoi);
        computer
            .ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<decimal?>(0.15m));

        var job = CreateJob([computer]);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        _dbContext.Signals.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Execute_WhenRecentTradesExist_ShouldComputeBucketsAndCreateSignals()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Market",
            new Taxes(null)
        );
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "ITEM1",
            null,
            "Item One",
            market.Id,
            new AdditionalFields()
        );

        var now = DateTimeOffset.UtcNow;
        var trades = Enumerable
            .Range(0, 5)
            .Select(i =>
                Trade.Create(
                    new Id<Trade>(Guid.NewGuid().ToString()),
                    symbol.Id,
                    100m + i,
                    10m,
                    now.AddHours(-i)
                )
            )
            .ToArray();

        var computer = Substitute.For<ISignalComputer>();
        computer.Type.Returns(SignalType.TaxAdjustedRoi);
        computer
            .ComputeAsync(
                Arg.Is<SignalComputeContext>(ctx => ctx.PriceBuckets.Count > 0),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<decimal?>(0.10m));

        var job = CreateJob([computer]);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trades);

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        await computer
            .Received()
            .ComputeAsync(
                Arg.Is<SignalComputeContext>(ctx => ctx.PriceBuckets.Count > 0),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Execute_WhenAllTradesHaveSameTimestamp_ShouldProduceSingleBucket()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Market",
            new Taxes(null)
        );
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            "ITEM2",
            null,
            "Item Two",
            market.Id,
            new AdditionalFields()
        );

        var sameTime = DateTimeOffset.UtcNow;
        var trades = new[]
        {
            Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 100m, 5m, sameTime),
            Trade.Create(new Id<Trade>(Guid.NewGuid().ToString()), symbol.Id, 110m, 3m, sameTime),
        };

        IReadOnlyList<PriceBucket>? capturedBuckets = null;
        var computer = Substitute.For<ISignalComputer>();
        computer.Type.Returns(SignalType.TaxAdjustedRoi);
        computer
            .ComputeAsync(
                Arg.Do<SignalComputeContext>(ctx => capturedBuckets = ctx.PriceBuckets),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<decimal?>(0.05m));

        var job = CreateJob([computer]);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(trades);

        // Act
        await job.Execute(_context, CancellationToken.None);

        // Assert
        capturedBuckets.ShouldNotBeNull();
        capturedBuckets!.Count.ShouldBe(1);
        capturedBuckets[0].Volume.ShouldBe(8m);
    }
}
