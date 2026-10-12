using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies;

public sealed class StrategyTests
{
    private readonly Fixture _fixture = new();

    public StrategyTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    public static TheoryData<StrategyConfiguration> InvalidConfigurations =>
        new()
        {
            new SwingConfiguration([]),
            new SwingConfiguration([
                new InputWeight(InputKind.SignalTaxAdjustedRoi, 0m),
                new InputWeight(InputKind.SignalRsi, 0m),
            ]),
            new SwingConfiguration([
                new InputWeight(InputKind.SignalRsi, 1m),
                new InputWeight(InputKind.SignalRsi, 2m),
            ]),
            new FlipConfiguration(CheckInHours: 0),
            new FlipConfiguration(MaxOpenOrders: 0),
            new FlipConfiguration(LookbackHours: -1),
            new FlipConfiguration(MaxHoldHours: 0),
            new FlipConfiguration(MaxPositionPercent: 0m),
            new FlipConfiguration(MaxPositionPercent: 1.5m),
            new FlipConfiguration(MaxPriceMove: -0.1m),
            new FlipConfiguration(MinPrice: -1m),
        };

    [Fact]
    public void Create_WhenValidSwingConfiguration_ShouldSetProperties()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();

        var configuration = new SwingConfiguration(
            StrategyTestFactory.DefaultWeights(),
            BuyThreshold: 0.1m,
            SellThreshold: -0.1m
        );

        // Act
        var strategy = Strategy.Create(marketId, name, description, configuration);

        // Assert
        strategy.Id.ShouldNotBe(default);
        strategy.MarketId.ShouldBe(marketId);
        strategy.Name.ShouldBe(name);
        strategy.Description.ShouldBe(description);
        strategy.Configuration.ShouldBeSameAs(configuration);
        strategy.Configuration.Mode.ShouldBe(StrategyMode.Swing);
        strategy.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Create_WhenValidFlipConfiguration_ShouldSetFlipMode()
    {
        // Arrange
        var configuration = new FlipConfiguration();

        // Act
        var strategy = Strategy.Create(_fixture.Create<Id<Market>>(), "Flip", null, configuration);

        // Assert
        strategy.Configuration.ShouldBeSameAs(configuration);
        strategy.Configuration.Mode.ShouldBe(StrategyMode.Flip);
    }

    [Fact]
    public void Create_WhenMarketMatches_ShouldExposeMarket()
    {
        // Arrange
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Market", new Taxes(null));

        // Act
        var strategy = Strategy.Create(
            market.Id,
            "Test",
            null,
            StrategyTestFactory.Swing(),
            market
        );

        // Assert
        strategy.Market.ShouldBeSameAs(market);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenNameIsInvalid_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();

        // Act
        var create = () => Strategy.Create(marketId, name!, null, StrategyTestFactory.Swing());

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();

        // Act
        var create = () => Strategy.Create(marketId, "Test", null, null!);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenSwingInputWeightsAreNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();

        // Act
        var create = () => Strategy.Create(marketId, "Test", null, new SwingConfiguration(null!));

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(InvalidConfigurations))]
    public void Create_WhenConfigurationIsInvalid_ShouldThrowArgumentException(
        StrategyConfiguration configuration
    )
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();

        // Act
        var create = () => Strategy.Create(marketId, "Test", null, configuration);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenMarketMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();

        var wrongMarket = Market.Create(
            _fixture.Create<Id<Market>>(),
            _fixture.Create<string>(),
            new Taxes(null)
        );

        // Act
        var create = () =>
            Strategy.Create(marketId, "Test", null, StrategyTestFactory.Swing(), wrongMarket);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenValidInput_ShouldChangeProperties()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            StrategyTestFactory.Swing()
        );

        var configuration = new SwingConfiguration(
            [new InputWeight(InputKind.SignalRsi, 2m)],
            BuyThreshold: 0.2m
        );

        // Act
        strategy.Update("Updated", "New description", configuration);

        // Assert
        strategy.Name.ShouldBe("Updated");
        strategy.Description.ShouldBe("New description");
        strategy.Configuration.ShouldBeSameAs(configuration);
    }

    [Fact]
    public void Update_WhenSwitchingMode_ShouldReplaceConfiguration()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Swing",
            null,
            StrategyTestFactory.Swing()
        );

        var configuration = new FlipConfiguration(MaxOpenOrders: 4);

        // Act
        strategy.Update("Flip", null, configuration);

        // Assert
        strategy.Configuration.Mode.ShouldBe(StrategyMode.Flip);
        strategy.Configuration.ShouldBeSameAs(configuration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenNameIsInvalid_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            StrategyTestFactory.Swing()
        );

        // Act
        var update = () => strategy.Update(name!, null, StrategyTestFactory.Swing());

        // Assert
        update.ShouldThrow<ArgumentException>();
        strategy.Name.ShouldBe("Original");
    }

    [Fact]
    public void Update_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            StrategyTestFactory.Swing()
        );

        // Act
        var update = () => strategy.Update("Updated", null, null!);

        // Assert
        update.ShouldThrow<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(InvalidConfigurations))]
    public void Update_WhenConfigurationIsInvalid_ShouldThrowAndKeepPreviousConfiguration(
        StrategyConfiguration configuration
    )
    {
        // Arrange
        var original = StrategyTestFactory.Swing();
        var strategy = Strategy.Create(_fixture.Create<Id<Market>>(), "Original", null, original);

        // Act
        var update = () => strategy.Update("Updated", null, configuration);

        // Assert
        update.ShouldThrow<ArgumentException>();
        strategy.Name.ShouldBe("Original");
        strategy.Configuration.ShouldBeSameAs(original);
    }

    [Fact]
    public void SetActive_WhenSetFalse_ShouldDeactivate()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        // Act
        strategy.SetActive(false);

        // Assert
        strategy.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void SetActive_WhenSetTrue_ShouldActivate()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        strategy.SetActive(false);

        // Act
        strategy.SetActive(true);

        // Assert
        strategy.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Market_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyTestFactory.Swing()
        );

        // Act
        var access = () => _ = strategy.Market;

        // Assert
        access.ShouldThrow<NavigationPropertyNotLoadedException<Strategy>>();
    }
}
