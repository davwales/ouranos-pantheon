using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Tests.Markets;

public sealed class MarketTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Market>(_fixture.Create<string>());
        var name = _fixture.Create<string>();
        var taxes = _fixture.Create<Taxes>();
        var isForecastingEnabled = _fixture.Create<bool>();
        var description = _fixture.Create<string>();
        var icon = _fixture.Create<string>();

        // Act
        var market = new Market(id, name, taxes, isForecastingEnabled, description, icon);

        // Assert
        market.Id.ShouldBe(id);
        market.Name.ShouldBe(name);
        market.Taxes.ShouldBe(taxes);
        market.IsForecastingEnabled.ShouldBe(isForecastingEnabled);
        market.Description.ShouldBe(description);
        market.Icon.ShouldBe(icon);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = new Id<Market>(_fixture.Create<string>());
        var taxes = _fixture.Create<Taxes>();

        // Act
        var create = () => new Market(id, name!, taxes);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullTaxes_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Market>(_fixture.Create<string>());
        var name = _fixture.Create<string>();

        // Act
        var create = () => new Market(id, name, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var market = new Market(
            new Id<Market>(_fixture.Create<string>()),
            "Original Name",
            _fixture.Create<Taxes>(),
            false,
            "Original Description",
            "original_icon.png"
        );
        const string newName = "Updated Name";
        var newTaxes = _fixture.Create<Taxes>();

        // Act
        market.Update(newName, newTaxes);

        // Assert
        market.Name.ShouldBe(newName);
        market.Taxes.ShouldBe(newTaxes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var market = new Market(
            new Id<Market>(_fixture.Create<string>()),
            "Original Name",
            _fixture.Create<Taxes>()
        );

        // Act
        var update = () => market.Update(name!, _fixture.Create<Taxes>());

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenNullTaxes_ShouldThrowArgumentException()
    {
        // Arrange
        var market = new Market(
            new Id<Market>(_fixture.Create<string>()),
            "Original Name",
            _fixture.Create<Taxes>()
        );

        // Act
        var update = () => market.Update("Valid Name", null!);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }
}