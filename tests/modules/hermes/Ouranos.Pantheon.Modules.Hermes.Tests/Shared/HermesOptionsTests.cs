using Ouranos.Pantheon.Modules.Hermes.Shared;

namespace Ouranos.Pantheon.Modules.Hermes.Tests.Shared;

public sealed class HermesOptionsTests
{
    [Theory]
    [InlineData(0, HermesOptions.DefaultCompactionMaxInputCharacters)]
    [InlineData(-5, HermesOptions.DefaultCompactionMaxInputCharacters)]
    [InlineData(1234, 1234)]
    public void EffectiveCompactionMaxInputCharacters_WhenConfigured_ShouldFallBackToDefaultWhenNotPositive(
        int configured,
        int expected
    )
    {
        // Arrange
        var options = new HermesOptions() with
        {
            CompactionMaxInputCharacters = configured,
        };

        // Act
        var result = options.EffectiveCompactionMaxInputCharacters;

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Constructor_WhenParameterless_ShouldUseDefaultCompactionMaxInputCharacters()
    {
        // Arrange
        var options = new HermesOptions();

        // Act
        var result = options.CompactionMaxInputCharacters;

        // Assert
        result.ShouldBe(HermesOptions.DefaultCompactionMaxInputCharacters);
    }
}
