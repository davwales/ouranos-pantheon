using Ouranos.Pantheon.Modules.Plutus.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared;

public sealed class MarketAnalystOptionsTests
{
    [Theory]
    [InlineData(null, 10)]
    [InlineData(5, 5)]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(50, 12)]
    public void ClampRows_ShouldDefaultAndKeepWithinOneAndMaxViewRows(int? requested, int expected)
    {
        // Arrange
        var options = new MarketAnalystOptions();

        // Act
        var rows = options.ClampRows(requested);

        // Assert
        rows.ShouldBe(expected);
    }
}
