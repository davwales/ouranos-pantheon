using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class AssistantNumberFormatTests
{
    [Theory]
    [InlineData(950, "950")]
    [InlineData(9999.5, "9999.5")]
    [InlineData(12345, "12.3K")]
    [InlineData(820592.33, "820.6K")]
    [InlineData(-45000, "-45K")]
    [InlineData(2500000, "2.5M")]
    [InlineData(1234567, "1.23M")]
    [InlineData(1200000000, "1.2B")]
    [InlineData(5600000000000, "5.6T")]
    public void Compact_ShouldAbbreviateLargeNumbers(decimal value, string expected)
    {
        // Act
        var formatted = AssistantNumberFormat.Compact(value);

        // Assert
        formatted.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0.0145, "1.5%")]
    [InlineData(-0.2, "-20%")]
    public void Percent_ShouldRenderOneDecimalPlace(decimal ratio, string expected)
    {
        // Act
        var formatted = AssistantNumberFormat.Percent(ratio);

        // Assert
        formatted.ShouldBe(expected);
    }

    [Fact]
    public void Number_ShouldUseInvariantCultureWithTwoDecimalPlaces()
    {
        // Act
        var formatted = AssistantNumberFormat.Number(1234.567m);

        // Assert
        formatted.ShouldBe("1234.57");
    }
}
