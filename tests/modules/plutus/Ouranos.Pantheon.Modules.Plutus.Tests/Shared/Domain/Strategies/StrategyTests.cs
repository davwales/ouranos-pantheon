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
        var config = new StrategyConfiguration();

        // Act
        var strategy = Strategy.Create(marketId, name, description, type, config);

        // Assert
        strategy.Id.ShouldNotBe(default);
        strategy.MarketId.ShouldBe(marketId);
        strategy.Name.ShouldBe(name);
        strategy.Description.ShouldBe(description);
        strategy.Type.ShouldBe(type);
        strategy.Configuration.ShouldBe(config);
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
        var config = new StrategyConfiguration();

        // Act
        var create = () => Strategy.Create(marketId, name!, null, StrategyType.SignalWeighted, config);

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
        var create = () => Strategy.Create(marketId, name, null, StrategyType.SignalWeighted, null!);

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
            new StrategyConfiguration(),
            wrongMarket
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
            new StrategyConfiguration()
        );
        var newConfig = new StrategyConfiguration(BuyThreshold: 0.5m);

        // Act
        strategy.Update("Updated", "New description", newConfig);

        // Assert
        strategy.Name.ShouldBe("Updated");
        strategy.Description.ShouldBe("New description");
        strategy.Configuration.ShouldBe(newConfig);
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
            new StrategyConfiguration()
        );

        // Act
        var update = () => strategy.Update(name!, null, new StrategyConfiguration());

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
            new StrategyConfiguration()
        );

        // Act
        var update = () => strategy.Update("Updated", null, null!);

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
            new StrategyConfiguration()
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
            new StrategyConfiguration()
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
            new StrategyConfiguration()
        );

        // Act
        var access = () => _ = strategy.Market;

        // Assert
        access.ShouldThrow<Exception>();
    }
}