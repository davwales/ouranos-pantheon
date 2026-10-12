using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies.Engine;

public sealed class StrategyEnginesTests
{
    private static IStrategyEngine Engine(StrategyMode mode)
    {
        var engine = Substitute.For<IStrategyEngine>();
        engine.Mode.Returns(mode);
        return engine;
    }

    [Fact]
    public void For_ShouldReturnTheEngineForTheConfigurationsMode()
    {
        // Arrange
        var swing = Engine(StrategyMode.Swing);
        var flip = Engine(StrategyMode.Flip);
        var engines = new StrategyEngines([swing, flip]);

        // Act
        var swingEngine = engines.For(StrategyTestFactory.Swing());
        var flipEngine = engines.For(new FlipConfiguration());

        // Assert
        swingEngine.ShouldBeSameAs(swing);
        flipEngine.ShouldBeSameAs(flip);
    }

    [Fact]
    public void For_WhenNoEngineHandlesTheMode_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var engines = new StrategyEngines([Engine(StrategyMode.Swing)]);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => engines.For(new FlipConfiguration()));
    }

    [Fact]
    public void For_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var engines = new StrategyEngines([Engine(StrategyMode.Swing)]);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => engines.For(null!));
    }

    [Fact]
    public void Constructor_WhenTwoEnginesShareAMode_ShouldThrowArgumentException()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new StrategyEngines([Engine(StrategyMode.Flip), Engine(StrategyMode.Flip)])
        );
    }

    [Fact]
    public void Constructor_WhenEnginesAreNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new StrategyEngines(null!));
    }
}
