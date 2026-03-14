using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Tests.Symbols;

public sealed class SymbolTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = _fixture.Create<Id<Symbol>>();
        var code = _fixture.Create<string>();
        var subcode = _fixture.Create<string>();
        var name = _fixture.Create<string>();
        var market = _fixture.Create<Market>();
        var additionalFields = new AdditionalFields();

        // Act
        var symbol = Symbol.Create(id, code, subcode, name, market, additionalFields);

        // Assert
        symbol.Id.ShouldBe(id);
        symbol.Code.ShouldBe(code);
        symbol.Subcode.ShouldBe(subcode);
        symbol.Name.ShouldBe(name);
        symbol.MarketId.ShouldBe(market.Id);
        symbol.AdditionalFields.ShouldBe(additionalFields);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidCode_ShouldThrowArgumentException(string? code)
    {
        // Arrange
        var id = _fixture.Create<Id<Symbol>>();
        var name = _fixture.Create<string>();
        var market = _fixture.Create<Market>();
        var additionalFields = new AdditionalFields();

        // Act
        var create = () => Symbol.Create(id, code!, null, name, market, additionalFields);

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
        var id = _fixture.Create<Id<Symbol>>();
        var code = _fixture.Create<string>();
        var market = _fixture.Create<Market>();
        var additionalFields = new AdditionalFields();

        // Act
        var create = () => Symbol.Create(id, code, null, name!, market, additionalFields);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullAdditionalFields_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<Symbol>>();
        var code = _fixture.Create<string>();
        var name = _fixture.Create<string>();
        var market = _fixture.Create<Market>();

        // Act
        var create = () => Symbol.Create(id, code, null, name, market, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            _fixture.Create<string>(),
            null,
            "Original Name",
            _fixture.Create<Market>(),
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
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            _fixture.Create<string>(),
            null,
            "Original Name",
            _fixture.Create<Market>(),
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
        var symbol = Symbol.Create(
            _fixture.Create<Id<Symbol>>(),
            _fixture.Create<string>(),
            null,
            "Original Name",
            _fixture.Create<Market>(),
            new AdditionalFields()
        );

        // Act
        var update = () => symbol.Update("Name", null!);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }
}