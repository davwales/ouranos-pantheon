using System.Globalization;
using System.Text.Json;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Tests.Application.Assistants;

public sealed class SignificantFiguresDecimalConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new SignificantFiguresDecimalConverter() },
    };

    [Theory]
    [InlineData("0", "0")]
    [InlineData("10266666.6667", "10270000")]
    [InlineData("1234567", "1235000")]
    [InlineData("-98765.43", "-98770")]
    [InlineData("123.45", "123.5")]
    [InlineData("0.0145678", "0.01457")]
    [InlineData("2.500", "2.5")]
    [InlineData("40", "40")]
    public void Write_WhenGivenADecimal_ShouldRoundToFourSignificantFigures(
        string value,
        string expected
    )
    {
        // Arrange
        var number = decimal.Parse(value, CultureInfo.InvariantCulture);

        // Act
        var json = JsonSerializer.Serialize(number, Options);

        // Assert
        json.ShouldBe(expected);
    }

    [Fact]
    public void Read_WhenGivenANumber_ShouldReadItUnchanged()
    {
        // Act
        var value = JsonSerializer.Deserialize<decimal>("10266666.6667", Options);

        // Assert
        value.ShouldBe(10266666.6667m);
    }
}
