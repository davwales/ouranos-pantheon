using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Tests.Symbols;

public sealed class SymbolTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Symbol>(_fixture.Create<string>());
        var code = _fixture.Create<string>();
        var subcode = _fixture.Create<string>();
        var name = _fixture.Create<string>();
        var marketId = new Id<Market>(_fixture.Create<string>());
        var additionalFields = new AdditionalFields();

        // Act
        var symbol = new Symbol(id, code, subcode, name, marketId, additionalFields);

        // Assert
        symbol.Id.ShouldBe(id);
        symbol.Code.ShouldBe(code);
        symbol.Subcode.ShouldBe(subcode);
        symbol.Name.ShouldBe(name);
        symbol.MarketId.ShouldBe(marketId);
        symbol.AdditionalFields.ShouldBe(additionalFields);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidCode_ShouldThrowArgumentException(string? code)
    {
        // Arrange
        var id = new Id<Symbol>(_fixture.Create<string>());
        var name = _fixture.Create<string>();
        var marketId = new Id<Market>(_fixture.Create<string>());
        var additionalFields = new AdditionalFields();

        // Act
        var create = () => new Symbol(id, code!, null, name, marketId, additionalFields);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = new Id<Symbol>(_fixture.Create<string>());
        var code = _fixture.Create<string>();
        var marketId = new Id<Market>(_fixture.Create<string>());
        var additionalFields = new AdditionalFields();

        // Act
        var create = () => new Symbol(id, code, null, name!, marketId, additionalFields);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullAdditionalFields_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Symbol>(_fixture.Create<string>());
        var code = _fixture.Create<string>();
        var name = _fixture.Create<string>();
        var marketId = new Id<Market>(_fixture.Create<string>());

        // Act
        var create = () => new Symbol(id, code, null, name, marketId, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var symbol = new Symbol(
            new Id<Symbol>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            "Original Name",
            new Id<Market>(_fixture.Create<string>()),
            new AdditionalFields()
        );
        const string newName = "Updated Name";
        var newFields = new AdditionalFields(1000);

        // Act
        symbol.Update(newName, newFields);

        // Assert
        symbol.Name.ShouldBe(newName);
        symbol.AdditionalFields.ShouldBe(newFields);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var symbol = new Symbol(
            new Id<Symbol>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            "Original Name",
            new Id<Market>(_fixture.Create<string>()),
            new AdditionalFields()
        );

        // Act
        var update = () => symbol.Update(name!, new AdditionalFields());

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenNullAdditionalFields_ShouldThrowArgumentException()
    {
        // Arrange
        var symbol = new Symbol(
            new Id<Symbol>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            "Original Name",
            new Id<Market>(_fixture.Create<string>()),
            new AdditionalFields()
        );

        // Act
        var update = () => symbol.Update("Name", null!);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }
}