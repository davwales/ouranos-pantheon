using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Tests.SymbolGroups;

public sealed class SymbolGroupTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = _fixture.Create<Id<SymbolGroup>>();
        var market = _fixture.Create<Market>();
        var name = _fixture.Create<string>();
        var symbols = _fixture.CreateMany<Symbol>().ToList();

        // Act
        var symbolGroup = SymbolGroup.Create(id, market, name, symbols);

        // Assert
        symbolGroup.Id.ShouldBe(id);
        symbolGroup.MarketId.ShouldBe(market.Id);
        symbolGroup.Name.ShouldBe(name);
        symbolGroup.SymbolIds.ShouldBe([.. symbols.Select(s => s.Id)]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = _fixture.Create<Id<SymbolGroup>>();
        var market = _fixture.Create<Market>();
        var symbols = _fixture.CreateMany<Symbol>().ToList();

        // Act
        var create = () => SymbolGroup.Create(id, market, name!, symbols);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullSymbolIds_ShouldThrowArgumentException()
    {
        // Arrange
        var id = _fixture.Create<Id<SymbolGroup>>();
        var market = _fixture.Create<Market>();
        var name = _fixture.Create<string>();

        // Act
        var create = () => SymbolGroup.Create(id, market, name, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}