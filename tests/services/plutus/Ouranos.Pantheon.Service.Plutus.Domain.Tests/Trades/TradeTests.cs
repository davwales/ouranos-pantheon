using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Tests.Trades;

public sealed class TradeTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_WhenHappyPath_ShouldSetExpectedProperties()
    {
        // Arrange
        var id = new Id<Trade>(_fixture.Create<string>());
        var price = _fixture.Create<decimal>();
        var volume = _fixture.Create<decimal>();
        var metadata = _fixture.Create<TradeMetadata>();
        var timestamp = _fixture.Create<DateTimeOffset>();

        // Act
        var trade = new Trade(id, price, volume, metadata, timestamp);

        // Assert
        trade.Id.ShouldBe(id);
        trade.Price.ShouldBe(price);
        trade.Volume.ShouldBe(volume);
        trade.Metadata.ShouldBe(metadata);
        trade.CreatedAt.ShouldBe(timestamp);
    }

    [Fact]
    public void Constructor_WhenNullMetadata_ShouldThrowArgumentException()
    {
        // Arrange
        var id = new Id<Trade>(_fixture.Create<string>());
        var price = _fixture.Create<decimal>();
        var volume = _fixture.Create<decimal>();
        TradeMetadata? metadata = null;
        var timestamp = _fixture.Create<DateTimeOffset>();

        // Act
        var create = () => new Trade(id, price, volume, metadata!, timestamp);

        // Assert
        create.ShouldThrow<ArgumentException>();
    }
}