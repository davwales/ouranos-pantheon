using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared.Domain.SymbolGroups;

public sealed class SymbolGroupTests
{
    private readonly IFixture _fixture = new Fixture();

    public SymbolGroupTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Fact]
    public void Create_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = _fixture.Create<Id<SymbolGroup>>();
        var name = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        var marketId = _fixture.Create<Id<Market>>();

        // Act
        var group = SymbolGroup.Create(id, name, description, marketId);

        // Assert
        group.Id.ShouldBe(id);
        group.Name.ShouldBe(name);
        group.Description.ShouldBe(description);
        group.MarketId.ShouldBe(marketId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var id = _fixture.Create<Id<SymbolGroup>>();
        var marketId = _fixture.Create<Id<Market>>();

        // Act
        var create = () => SymbolGroup.Create(id, name!, null, marketId);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenMarketNavigationMismatch_ShouldThrow()
    {
        // Arrange
        var id = _fixture.Create<Id<SymbolGroup>>();
        var marketId = _fixture.Create<Id<Market>>();
        var wrongMarket = Market.Create(
            new Id<Market>(_fixture.Create<string>()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        // Act
        var create = () =>
            SymbolGroup.Create(id, _fixture.Create<string>(), null, marketId, wrongMarket);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Update_WhenHappyPath_ShouldUpdateProperties()
    {
        // Arrange
        var group = SymbolGroup.Create(
            _fixture.Create<Id<SymbolGroup>>(),
            "Original Name",
            "Original Description",
            _fixture.Create<Id<Market>>()
        );
        const string newName = "Updated Name";
        const string newDescription = "Updated Description";

        // Act
        group.Update(newName, newDescription);

        // Assert
        group.Name.ShouldBe(newName);
        group.Description.ShouldBe(newDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WhenInvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var group = SymbolGroup.Create(
            _fixture.Create<Id<SymbolGroup>>(),
            "Original Name",
            null,
            _fixture.Create<Id<Market>>()
        );

        // Act
        var update = () => group.Update(name!, null);

        // Assert
        update.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Market_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var group = SymbolGroup.Create(
            _fixture.Create<Id<SymbolGroup>>(),
            "Test Group",
            null,
            _fixture.Create<Id<Market>>()
        );

        // Act
        var access = () => _ = group.Market;

        // Assert
        access.ShouldThrow<Exception>();
    }

    [Fact]
    public void Members_WhenNotLoaded_ShouldThrowNavigationPropertyNotLoadedException()
    {
        // Arrange
        var group = SymbolGroup.Create(
            _fixture.Create<Id<SymbolGroup>>(),
            "Test Group",
            null,
            _fixture.Create<Id<Market>>()
        );

        // Act
        var access = () => _ = group.Members;

        // Assert
        access.ShouldThrow<Exception>();
    }

    [Fact]
    public void Create_WhenMarketIdMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        var differentMarketId = _fixture.Create<Id<Market>>();

        // Act
        var create = () =>
            SymbolGroup.Create(
                _fixture.Create<Id<SymbolGroup>>(),
                "Test Group",
                null,
                differentMarketId,
                market
            );

        // Assert
        create.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Create_WhenMarketAndMembersProvided_ShouldExposeNavigationProperties()
    {
        // Arrange
        var market = _fixture.Create<Market>();
        var members = new List<SymbolGroupMember>();

        // Act
        var group = SymbolGroup.Create(
            _fixture.Create<Id<SymbolGroup>>(),
            "Test Group",
            null,
            market.Id,
            market,
            members
        );

        // Assert
        group.Market.ShouldBe(market);
        group.Members.ShouldBe(members);
    }
}
