using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
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
        var type = StrategyType.SignalWeighted;
        var config = new TradingConfiguration();

        // Act
        var strategy = Strategy.Create(marketId, name, description, type, config, new SignalWeightedConfig());

        // Assert
        strategy.Id.ShouldNotBe(default);
        strategy.MarketId.ShouldBe(marketId);
        strategy.Name.ShouldBe(name);
        strategy.Description.ShouldBe(description);
        strategy.Type.ShouldBe(type);
        strategy.TradingConfiguration.ShouldBe(config);
        strategy.SignalWeightedConfig.ShouldNotBeNull();
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
        var create = () => Strategy.Create(
            marketId,
            name!,
            null,
            StrategyType.SignalWeighted,
            config,
            new SignalWeightedConfig()
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
        var create = () => Strategy.Create(
            marketId,
            name,
            null,
            StrategyType.SignalWeighted,
            null!,
            new SignalWeightedConfig()
        );

        // Assert
        create.ShouldThrow<ArgumentNullException>();
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
        var create = () => Strategy.Create(
            marketId,
            _fixture.Create<string>(),
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig(),
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
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );
        var newConfig = new TradingConfiguration();

        // Act
        strategy.Update("Updated", "New description", newConfig, new SignalWeightedConfig());

        // Assert
        strategy.Name.ShouldBe("Updated");
        strategy.Description.ShouldBe("New description");
        strategy.TradingConfiguration.ShouldBe(newConfig);
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
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );

        // Act
        var update = () => strategy.Update(name!, null, new TradingConfiguration(), new SignalWeightedConfig());

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
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );

        // Act
        var update = () => strategy.Update("Updated", null, null!, new SignalWeightedConfig());

        // Assert
        update.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void SetActive_WhenSetFalse_ShouldDeactivate()
    {
        // Arrange
        var strategy = Strategy.Create(
            _fixture.Create<Id<Market>>(),
            "Test",
            null,
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
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
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
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
            StrategyType.SignalWeighted,
            new TradingConfiguration(),
            new SignalWeightedConfig()
        );

        // Act
        var access = () => _ = strategy.Market;

        // Assert
        access.ShouldThrow<Exception>();
    }
}
