using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.Strategies;

public sealed class StrategyTests
{
    private readonly IFixture _fixture = new Fixture();

    public StrategyTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenValidInput_ShouldSetProperties()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var config = new TradingConfiguration();
        var weights = StrategyTestFactory.DefaultWeights();
        var thresholds = new InputThresholds(BuyThreshold: 0.1m, SellThreshold: -0.1m);

        // Act
        var strategy = Strategy.Create(marketId, name, description, config, weights, thresholds);

        // Assert
        strategy.Id.ShouldNotBe(default);
        strategy.MarketId.ShouldBe(marketId);
        strategy.Name.ShouldBe(name);
        strategy.Description.ShouldBe(description);
        strategy.TradingConfiguration.ShouldBe(config);
        strategy.InputWeights.ShouldBeSameAs(weights);
        strategy.Thresholds.ShouldBe(thresholds);
        strategy.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenNameIsInvalid_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var config = new TradingConfiguration();

        // Act
        var create = () =>
            Strategy.Create(
                marketId,
                name!,
                null,
                config,
                StrategyTestFactory.DefaultWeights(),
                null
            );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var name = _fixture.Create<string>();

        // Act
        var create = () =>
            Strategy.Create(
                marketId,
                name,
                null,
                null!,
                StrategyTestFactory.DefaultWeights(),
                null
            );

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenInputWeightsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var name = _fixture.Create<string>();

        // Act
        var create = () =>
            Strategy.Create(marketId, name, null, new TradingConfiguration(), null!, null);

        // Assert
        create.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_WhenAllWeightsAreZero_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var zeroWeights = new List<InputWeight>
        {
            new(InputKind.SignalTaxAdjustedRoi, 0m),
            new(InputKind.SignalRsi, 0m),
        };

        // Act
        var create = () =>
            Strategy.Create(
                marketId,
                _fixture.Create<string>(),
                null,
                new TradingConfiguration(),
                zeroWeights,
                null
            );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenMarketMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var marketId = _fixture.Create<Id<Market>>();
        var wrongMarket = Market.Create(
            new Id<Market>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        // Act
        var create = () =>
            Strategy.Create(
                marketId,
                _fixture.Create<string>(),
                null,
                new TradingConfiguration(),
                StrategyTestFactory.DefaultWeights(),
                null,
                market: wrongMarket
            );

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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var newConfig = new TradingConfiguration();
        var newWeights = new List<InputWeight> { new(InputKind.SignalRsi, 2m) };
        var newThresholds = new InputThresholds(BuyThreshold: 0.2m);

        // Act
        strategy.Update("Updated", "New description", newConfig, newWeights, newThresholds);

        // Assert
        strategy.Name.ShouldBe("Updated");
        strategy.Description.ShouldBe("New description");
        strategy.TradingConfiguration.ShouldBe(newConfig);
        strategy.InputWeights.ShouldBeSameAs(newWeights);
        strategy.Thresholds.ShouldBe(newThresholds);
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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );

        // Act
        var update = () =>
            strategy.Update(
                name!,
                null,
                new TradingConfiguration(),
                StrategyTestFactory.DefaultWeights(),
                null
            );

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );

        // Act
        var update = () =>
            strategy.Update("Updated", null, null!, StrategyTestFactory.DefaultWeights(), null);

        // Assert
        update.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Update_WhenAllWeightsAreZero_ShouldThrowArgumentException()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Original",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );
        var zeroWeights = new List<InputWeight> { new(InputKind.SignalTaxAdjustedRoi, 0m) };

        // Act
        var update = () =>
            strategy.Update("Updated", null, new TradingConfiguration(), zeroWeights, null);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void SetActive_WhenSetFalse_ShouldDeactivate()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
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
            new TradingConfiguration(),
            StrategyTestFactory.DefaultWeights(),
            null
        );

        // Act
        var access = () => _ = strategy.Market;

        // Assert
        access.ShouldThrow<Exception>();
    }
}
