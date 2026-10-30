using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Shared.Domain.ShoppingLists;

public sealed class MeasurementUnitsTests
{
    [Theory]
    [InlineData("tbsp", "tbsp")]
    [InlineData("Tablespoons", "tbsp")]
    [InlineData("tablespoon", "tbsp")]
    [InlineData("cups", "cup")]
    [InlineData("quarts", "quart")]
    [InlineData("fl. oz.", "fl oz")]
    [InlineData("lbs", "lb")]
    [InlineData(" Grams ", "g")]
    public void Find_WhenSpellingIsKnown_ShouldReturnCanonicalUnit(string spelling, string symbol)
    {
        // Act
        var unit = MeasurementUnits.Find(spelling);

        // Assert
        unit.ShouldNotBeNull();
        unit.Symbol.ShouldBe(symbol);
    }

    [Theory]
    [InlineData("whole")]
    [InlineData("cloves")]
    [InlineData("as needed")]
    public void Find_WhenUnitDoesNotConvert_ShouldReturnNull(string spelling)
    {
        // Act
        var unit = MeasurementUnits.Find(spelling);

        // Assert
        unit.ShouldBeNull();
    }

    [Fact]
    public void Find_WhenVolumeAndMassUnits_ShouldReportTheirDimension()
    {
        // Act
        var cup = MeasurementUnits.Find("cup");
        var ounce = MeasurementUnits.Find("oz");

        // Assert
        cup.ShouldNotBeNull();
        cup.Dimension.ShouldBe(UnitDimension.Volume);
        ounce.ShouldNotBeNull();
        ounce.Dimension.ShouldBe(UnitDimension.Mass);
    }
}
