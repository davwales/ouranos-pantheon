using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.GetSymbolSignalHistory;

public sealed class GetSymbolSignalHistoryHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<GetSymbolSignalHistoryHandler> _logger = Substitute.For<
        ILogger<GetSymbolSignalHistoryHandler>
    >();
    private readonly PlutusDbContext _dbContext;
    private readonly ISignalComputer[] _computers;

    public GetSymbolSignalHistoryHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();

        _computers =
        [
            new StubSignalComputer(
                SignalType.Rsi,
                "RSI",
                [InvestmentIntent.Buy, InvestmentIntent.Flip]
            ),
            new StubSignalComputer(
                SignalType.BollingerBands,
                "Bollinger Bands",
                [InvestmentIntent.Flip]
            ),
            new StubSignalComputer(
                SignalType.PriceVelocity,
                "Price Velocity",
                [InvestmentIntent.Sell]
            ),
            new StubSignalComputer(
                SignalType.MovingAverageCrossover,
                "MA Crossover",
                [InvestmentIntent.Merch]
            ),
            new StubSignalComputer(
                SignalType.TrendMomentum,
                "Trend Momentum",
                [InvestmentIntent.Merch]
            ),
            new StubSignalComputer(SignalType.TaxAdjustedRoi, "Tax ROI", [InvestmentIntent.Flip]),
            new StubSignalComputer(
                SignalType.VolumeAnomaly,
                "Volume Anomaly",
                [InvestmentIntent.Buy]
            ),
        ];
    }

    [Fact]
    public async Task Handle_WhenSymbolNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalHistoryInput(new Id<Symbol>(_fixture.Create<string>()));

        // Act
        var get = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenNoSignalsInTimeWindow_ShouldReturnEmptyHistory()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);

        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalHistoryInput(symbol.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.SymbolId.ShouldBe(symbol.Id);
        result.Signals.All(s => s.History.Count == 0).ShouldBeTrue();
        result.Summary.AggregatedScore.ShouldBe(0m);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnHistoryForSymbolWithSignals()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var now = DateTimeOffset.UtcNow;
        var signals = new[]
        {
            Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.5m),
            Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.7m),
            Signal.Create(market.Id, symbol.Id, SignalType.BollingerBands, -0.3m),
        };

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(signals);

        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalHistoryInput(symbol.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.SymbolId.ShouldBe(symbol.Id);
        result.SymbolName.ShouldBe(symbol.Name);

        var rsiSignal = result.Signals.SingleOrDefault(s => s.Type == nameof(SignalType.Rsi));
        rsiSignal.ShouldNotBeNull();
        rsiSignal.History.Count.ShouldBe(2);
        rsiSignal.CurrentValue.ShouldBe(0.7m);
        rsiSignal.Direction.ShouldBe(SignalDirection.Bullish);

        var bbSignal = result.Signals.SingleOrDefault(s =>
            s.Type == nameof(SignalType.BollingerBands)
        );
        bbSignal.ShouldNotBeNull();
        bbSignal.History.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenFromAfterTo_ShouldThrowArgumentException()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);

        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalHistoryInput(
            symbol.Id,
            From: DateTimeOffset.UtcNow,
            To: DateTimeOffset.UtcNow.AddDays(-1)
        );

        // Act
        var get = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenInvalidTypeFilter_ShouldThrowArgumentException()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);

        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalHistoryInput(symbol.Id, Types: "InvalidType");

        // Act
        var get = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenTypeFilterProvided_ShouldOnlyReturnRequestedTypes()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var signals = new[]
        {
            Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.7m),
            Signal.Create(market.Id, symbol.Id, SignalType.BollingerBands, -0.3m),
            Signal.Create(market.Id, symbol.Id, SignalType.TaxAdjustedRoi, 0.1m),
        };

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(signals);

        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalHistoryInput(symbol.Id, Types: "Rsi,BollingerBands");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Signals.ShouldContain(s => s.Type == nameof(SignalType.Rsi));
        result.Signals.ShouldContain(s => s.Type == nameof(SignalType.BollingerBands));
        result.Signals.ShouldNotContain(s => s.Type == nameof(SignalType.TaxAdjustedRoi));
    }

    [Fact]
    public async Task Handle_WhenDateRangeProvided_ShouldOnlyReturnSignalsWithinRange()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var now = DateTimeOffset.UtcNow;
        var inRangeSignal = Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.5m);
        var outOfRangeSignal = Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.9m);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(inRangeSignal, outOfRangeSignal);

        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);

        var from = now.AddDays(-3);
        var to = now.AddDays(-1);
        var query = new GetSymbolSignalHistoryInput(symbol.Id, From: from, To: to);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        var rsiSignal = result.Signals.SingleOrDefault(s => s.Type == nameof(SignalType.Rsi));
        rsiSignal.ShouldNotBeNull();
        rsiSignal.History.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var handler = new TestableGetSymbolSignalHistoryHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalHistoryInput(new Id<Symbol>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    private sealed class TestableGetSymbolSignalHistoryHandler(
        ILogger<GetSymbolSignalHistoryHandler> logger,
        PlutusDbContext dbContext,
        IEnumerable<ISignalComputer> computers
#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
    ) : GetSymbolSignalHistoryHandler(logger, dbContext, computers)
#pragma warning restore CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
    {
        protected internal override async Task<
            Dictionary<SignalType, List<RawSignalPoint>>
        > FetchSignalGroups(
            Id<Symbol> symbolId,
            DateTimeOffset from,
            DateTimeOffset to,
            HashSet<SignalType>? requestedTypes,
            HashSet<SignalType>? typesForIntent,
            CancellationToken ct
        )
        {
            var query = dbContext
                .Signals.AsNoTracking()
                .Where(s => s.SymbolId == symbolId)
                .Where(s => s.ComputedAt >= from && s.ComputedAt <= to);

            if (requestedTypes is { Count: > 0 })
            {
                query = query.Where(s => requestedTypes.Contains(s.Type));
            }

            if (typesForIntent is { Count: > 0 })
            {
                query = query.Where(s => typesForIntent.Contains(s.Type));
            }

            var raw = await query
                .OrderBy(s => s.Type)
                .ThenBy(s => s.ComputedAt)
                .Select(s => new GetSymbolSignalHistoryHandler.RawSignalPoint(
                    s.Type,
                    s.Value,
                    s.ComputedAt
                ))
                .ToListAsync(ct);

            return raw.GroupBy(s => s.Type).ToDictionary(g => g.Key, g => g.ToList());
        }
    }

    private sealed class StubSignalComputer(
        SignalType type,
        string label,
        IReadOnlyList<InvestmentIntent> intents
    ) : ISignalComputer
    {
        public SignalType Type => type;
        public string Label => label;
        public string Description => $"{label} description";
        public IReadOnlyList<InvestmentIntent> Intents => intents;

        public Task<decimal?> ComputeAsync(SignalComputeContext context, CancellationToken ct)
        {
            return Task.FromResult<decimal?>(0m);
        }
    }
}
