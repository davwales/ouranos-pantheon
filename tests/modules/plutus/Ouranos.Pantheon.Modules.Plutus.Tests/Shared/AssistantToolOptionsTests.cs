using Ouranos.Pantheon.Modules.Plutus.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared;

public sealed class AssistantToolOptionsTests
{
    [Theory]
    [InlineData(null, 10)]
    [InlineData(5, 5)]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(50, 12)]
    public void ClampRows_ShouldDefaultAndKeepWithinOneAndMaxRows(int? requested, int expected)
    {
        // Arrange
        var options = new AssistantToolOptions();

        // Act
        var rows = options.ClampRows(requested);

        // Assert
        rows.ShouldBe(expected);
    }
}
