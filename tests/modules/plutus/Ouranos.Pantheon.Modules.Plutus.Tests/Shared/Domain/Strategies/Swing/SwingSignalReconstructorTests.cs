using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Swing;

public sealed class SwingSignalReconstructorTests
{
    private static readonly DateOnly Day0 = SwingTestData.Day(SwingTestData.Start);

    private readonly Market _market = SwingTestData.CreateMarket(SwingTestData.OsrsTax);
    private readonly List<SignalComputeContext> _contexts = [];
    private readonly ISignalComputer _computer = Substitute.For<ISignalComputer>();

    public SwingSignalReconstructorTests()
    {
        _computer.Type.Returns(SignalType.Rsi);

        _computer
            .ComputeAsync(Arg.Do<SignalComputeContext>(_contexts.Add), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<decimal?>(0.4m));
    }

    [Fact]
    public void Constructor_WhenComputersAreNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new SwingSignalReconstructor(null!));
    }

    [Fact]
    public async Task BuildAsync_ShouldOnlySeeTradesBeforeTheDecisionDay()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");

        var aggregates = SwingTestData
            .Steady(a, Day0.AddDays(-20), Day0.AddDays(-1), 10m, 100m)
            .Append(new DailyTradeAggregate(a.Id, Day0, 999m, 999m, 999m, 100_000m));

        var data = SwingMarketData.FromRaw(_market, [a], [.. aggregates]);
        var reconstructor = new SwingSignalReconstructor([_computer]);

        // Act
        var table = await reconstructor.BuildAsync(data, Day0, Day0, CancellationToken.None);

        // Assert
        table.On(Day0)[a.Id].Price.ShouldBe(10m);
        var context = _contexts.ShouldHaveSingleItem();
        context.PriceBuckets.ShouldNotBeEmpty();
        context.PriceBuckets.ShouldAllBe(b => b.MaxPrice == 10m);
        context.PriceBuckets.ShouldAllBe(b => DateOnly.FromDateTime(b.BucketStart.DateTime) < Day0);
        context.MediumSnapshot.ShouldNotBeNull().MaxPrice.ShouldBe(10m);
    }

    [Fact]
    public async Task BuildAsync_ShouldPassTheSymbolBuyLimitAndMarketTaxRate()
    {
        // Arrange
        var limited = SwingTestData.CreateSymbol(_market, "Limited", limit: 70m);
        var unlimited = SwingTestData.CreateSymbol(_market, "Unlimited");
        Symbol[] symbols = [limited, unlimited];

        var data = SwingMarketData.FromRaw(
            _market,
            [.. symbols],
            [
                .. symbols.SelectMany(s =>
                    SwingTestData.Steady(s, Day0.AddDays(-5), Day0.AddDays(-1), 10m, 100m)
                ),
            ]
        );

        var reconstructor = new SwingSignalReconstructor([_computer]);

        // Act
        await reconstructor.BuildAsync(data, Day0, Day0, CancellationToken.None);

        // Assert
        _contexts.Count.ShouldBe(2);
        _contexts.Single(c => c.SymbolId == limited.Id).Limit.ShouldBe(70m);
        _contexts.Single(c => c.SymbolId == unlimited.Id).Limit.ShouldBe(0m);
        _contexts.ShouldAllBe(c => c.TaxRate == 0.02m);
    }

    [Fact]
    public async Task BuildAsync_WhenSymbolHasNoTradesInTheSignalWindow_ShouldSkipIt()
    {
        // Arrange
        var silent = SwingTestData.CreateSymbol(_market, "Silent");
        var stale = SwingTestData.CreateSymbol(_market, "Stale");
        var todayOnly = SwingTestData.CreateSymbol(_market, "TodayOnly");

        var data = SwingMarketData.FromRaw(
            _market,
            [silent, stale, todayOnly],
            [
                .. SwingTestData.Steady(stale, Day0.AddDays(-40), Day0.AddDays(-31), 10m, 100m),
                .. SwingTestData.Steady(todayOnly, Day0, Day0, 10m, 100m),
            ]
        );

        var reconstructor = new SwingSignalReconstructor([_computer]);

        // Act
        var table = await reconstructor.BuildAsync(data, Day0, Day0, CancellationToken.None);

        // Assert
        table.On(Day0).ShouldBeEmpty();
        _contexts.ShouldBeEmpty();
    }

    [Fact]
    public async Task BuildAsync_ShouldMarkSymbolsWithoutRecentVolumeIneligible()
    {
        // Arrange
        var liquid = SwingTestData.CreateSymbol(_market, "Liquid");
        var dormant = SwingTestData.CreateSymbol(_market, "Dormant");

        var data = SwingMarketData.FromRaw(
            _market,
            [liquid, dormant],
            [
                .. SwingTestData.Steady(liquid, Day0.AddDays(-14), Day0.AddDays(-1), 10m, 100m),
                .. SwingTestData.Steady(dormant, Day0.AddDays(-20), Day0.AddDays(-20), 10m, 100m),
            ]
        );

        var reconstructor = new SwingSignalReconstructor([_computer]);

        // Act
        var table = await reconstructor.BuildAsync(data, Day0, Day0, CancellationToken.None);

        // Assert
        table.On(Day0)[liquid.Id].IsEligible.ShouldBeTrue();
        var signals = table.On(Day0)[dormant.Id];
        signals.IsEligible.ShouldBeFalse();
        signals.Signals.ShouldContainKeyAndValue(SignalType.Rsi, 0.4m);
    }

    [Fact]
    public async Task BuildAsync_ShouldKeepOnlySignalsThatHadEnoughData()
    {
        // Arrange
        var missing = Substitute.For<ISignalComputer>();
        missing.Type.Returns(SignalType.PriceVelocity);

        missing
            .ComputeAsync(Arg.Any<SignalComputeContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<decimal?>(null));

        var a = SwingTestData.CreateSymbol(_market, "A");

        var data = SwingMarketData.FromRaw(
            _market,
            [a],
            [.. SwingTestData.Steady(a, Day0.AddDays(-5), Day0.AddDays(-1), 10m, 100m)]
        );

        var reconstructor = new SwingSignalReconstructor([_computer, missing]);

        // Act
        var table = await reconstructor.BuildAsync(data, Day0, Day0, CancellationToken.None);

        // Assert
        table
            .On(Day0)[a.Id]
            .Signals.ShouldBe(new Dictionary<SignalType, decimal> { [SignalType.Rsi] = 0.4m });
    }

    [Fact]
    public async Task BuildAsync_ShouldBuildEveryDayInTheRange()
    {
        // Arrange
        var a = SwingTestData.CreateSymbol(_market, "A");

        var data = SwingMarketData.FromRaw(
            _market,
            [a],
            [.. SwingTestData.Steady(a, Day0.AddDays(-5), Day0.AddDays(2), 10m, 100m)]
        );

        var reconstructor = new SwingSignalReconstructor([_computer]);

        // Act
        var table = await reconstructor.BuildAsync(
            data,
            Day0,
            Day0.AddDays(2),
            CancellationToken.None
        );

        // Assert
        table.On(Day0).ShouldContainKey(a.Id);
        table.On(Day0.AddDays(2)).ShouldContainKey(a.Id);
        table.On(Day0.AddDays(3)).ShouldBeEmpty();
        _contexts.Count.ShouldBe(3);
    }

    [Fact]
    public async Task BuildAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var data = SwingMarketData.FromRaw(_market, [], []);
        var reconstructor = new SwingSignalReconstructor([_computer]);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            reconstructor.BuildAsync(data, Day0, Day0, cancellation.Token)
        );
    }
}
