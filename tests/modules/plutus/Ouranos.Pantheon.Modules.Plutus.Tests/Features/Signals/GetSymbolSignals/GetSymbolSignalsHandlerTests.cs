using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.GetSymbolSignals;

public sealed class GetSymbolSignalsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<GetSymbolSignalsHandler> _logger = Substitute.For<
        ILogger<GetSymbolSignalsHandler>
    >();
    private readonly PlutusDbContext _dbContext;
    private readonly ISignalComputer[] _computers;

    public GetSymbolSignalsHandlerTests()
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
        var handler = new GetSymbolSignalsHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalsInput(new Id<Symbol>(_fixture.Create<string>()));

        // Act
        var get = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenNoSignals_ShouldReturnEmptySummary()
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

        var handler = new GetSymbolSignalsHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalsInput(symbol.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Signals.ShouldBeEmpty();
        result.Summary.AggregatedScore.ShouldBe(0m);
        result.Summary.BullishCount.ShouldBe(0);
        result.Summary.BearishCount.ShouldBe(0);
        result.Summary.NeutralCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenSignalsExist_ShouldMapDirectionAndStrength()
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

        var bullishSignal = Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.8m);
        var bearishSignal = Signal.Create(market.Id, symbol.Id, SignalType.BollingerBands, -0.5m);
        var neutralSignal = Signal.Create(market.Id, symbol.Id, SignalType.PriceVelocity, 0m);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(bullishSignal, bearishSignal, neutralSignal);

        var handler = new GetSymbolSignalsHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalsInput(symbol.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.SymbolId.ShouldBe(symbol.Id);
        result.SymbolName.ShouldNotBeNull();
        result.Signals.Count.ShouldBe(3);

        var bullish = result.Signals.Single(s => s.Value == 0.8m);
        bullish.Direction.ShouldBe(SignalDirection.Bullish);
        bullish.Strength.ShouldBe(SignalStrength.Strong);
        bullish.Type.ShouldNotBeNull();
        bullish.Label.ShouldNotBeNull();
        bullish.Description.ShouldNotBeNull();
        bullish.Intents.ShouldNotBeNull();

        var bearish = result.Signals.Single(s => s.Value == -0.5m);
        bearish.Direction.ShouldBe(SignalDirection.Bearish);
        bearish.Strength.ShouldBe(SignalStrength.Medium);

        var neutral = result.Signals.Single(s => s.Value == 0m);
        neutral.Direction.ShouldBe(SignalDirection.Neutral);
        neutral.Strength.ShouldBe(SignalStrength.Weak);
    }

    [Fact]
    public async Task Handle_WhenSignalsExist_ShouldComputeSummaryCorrectly()
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

        var signal1 = Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.8m);
        var signal2 = Signal.Create(market.Id, symbol.Id, SignalType.BollingerBands, -0.4m);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(signal1, signal2);

        var handler = new GetSymbolSignalsHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalsInput(symbol.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Summary.BullishCount.ShouldBe(1);
        result.Summary.BearishCount.ShouldBe(1);
        result.Summary.NeutralCount.ShouldBe(0);
        result.Summary.AggregatedScore.ShouldBe((0.8m + -0.4m) / 2m);
    }

    [Fact]
    public async Task Handle_WhenIntentProvided_ShouldFilterSignalsByMatchingComputers()
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

        var rsiSignal = Signal.Create(market.Id, symbol.Id, SignalType.Rsi, 0.7m);
        var bbSignal = Signal.Create(market.Id, symbol.Id, SignalType.BollingerBands, -0.3m);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(rsiSignal, bbSignal);

        var handler = new GetSymbolSignalsHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalsInput(symbol.Id, Intent: InvestmentIntent.Buy);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Signals.ShouldContain(s => s.Type == nameof(SignalType.Rsi));
        result.Signals.ShouldNotContain(s => s.Type == nameof(SignalType.BollingerBands));
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var handler = new GetSymbolSignalsHandler(_logger, _dbContext, _computers);
        var query = new GetSymbolSignalsInput(new Id<Symbol>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
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
