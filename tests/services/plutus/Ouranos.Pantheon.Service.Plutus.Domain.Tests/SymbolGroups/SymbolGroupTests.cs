using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Tests.SymbolGroups;

public sealed class SymbolGroupTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<SymbolGroup>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var name = _fixture.Create<string>();
        var symbolIds = _fixture.CreateMany<Id<Symbol>>().ToList();

        // Act
        var symbolGroup = new SymbolGroup(id, marketId, name, symbolIds);

        // Assert
        symbolGroup.Id.ShouldBe(id);
        symbolGroup.MarketId.ShouldBe(marketId);
        symbolGroup.Name.ShouldBe(name);
        symbolGroup.SymbolIds.ShouldBe(symbolIds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = new Id<SymbolGroup>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var symbolIds = _fixture.CreateMany<Id<Symbol>>().ToList();

        // Act
        var create = () => new SymbolGroup(id, marketId, name!, symbolIds);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenNullSymbolIds_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<SymbolGroup>(_fixture.Create<string>());
        var marketId = new Id<Market>(_fixture.Create<string>());
        var name = _fixture.Create<string>();

        // Act
        var create = () => new SymbolGroup(id, marketId, name, null!);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}