using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.SymbolGroups;

public sealed class SymbolGroupMemberTests
{
    private readonly IFixture _fixture = new Fixture();

    public SymbolGroupMemberTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var groupId = _fixture.Create<Id<SymbolGroup>>();
        var symbolId = _fixture.Create<Id<Symbol>>();

        // Act
        var member = SymbolGroupMember.Create(groupId, symbolId);

        // Assert
        member.SymbolGroupId.ShouldBe(groupId);
        member.SymbolId.ShouldBe(symbolId);
    }

    [Fact]
    public void Create_ShouldSetAddedAtToUtcNow()
    {
        // Arrange
        var groupId = _fixture.Create<Id<SymbolGroup>>();
        var symbolId = _fixture.Create<Id<Symbol>>();

        // Act
        var member = SymbolGroupMember.Create(groupId, symbolId);

        // Assert
        member.AddedAt.ShouldBe(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WhenNullSymbol_ShouldSucceed()
    {
        // Arrange
        var groupId = _fixture.Create<Id<SymbolGroup>>();
        var symbolId = _fixture.Create<Id<Symbol>>();

        // Act
        var member = SymbolGroupMember.Create(groupId, symbolId, null);

        // Assert
        member.ShouldNotBeNull();
    }

    [Fact]
    public void Create_WhenSymbolNavigationMismatch_ShouldThrow()
    {
        // Arrange
        var groupId = _fixture.Create<Id<SymbolGroup>>();
        var symbolId = _fixture.Create<Id<Symbol>>();
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );
        var wrongSymbol = Symbol.Create(
            new Id<Symbol>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        // Act
        var create = () => SymbolGroupMember.Create(groupId, symbolId, wrongSymbol);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}
