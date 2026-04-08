using System.Text;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Ffxiv.XivApi;

public sealed class ItemParserTests
{
    private static Stream CreateCsvStream(string csv)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(csv));
    }

    [Fact]
    public void ParseItemCsv_WhenValidCsv_ShouldReturnItems()
    {
        // Arrange
        var header1 = "junk1";
        var header2 = "junk2";
        var header3 = "junk3";

        string MakeRow(string key, string name, string canBeHq)
        {
            var cols = Enumerable.Repeat("", 89).ToArray();
            cols[0] = key;
            cols[4] = name;
            cols[88] = canBeHq;
            return string.Join(",", cols);
        }

        var headerRow = string.Join(",", Enumerable.Repeat("col", 89));
        var row1 = MakeRow("1", "Sword", "True");
        var row2 = MakeRow("2", "Shield", "False");
        var csv = string.Join("\n", header1, header2, header3, headerRow, row1, row2);

        using var stream = CreateCsvStream(csv);

        // Act
        var result = ItemParser.ParseItemCsv(stream);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].Key.ShouldBe(1);
        result[0].Name.ShouldBe("Sword");
        result[0].CanBeHq.ShouldBeTrue();
        result[1].Key.ShouldBe(2);
        result[1].Name.ShouldBe("Shield");
        result[1].CanBeHq.ShouldBeFalse();
    }

    [Fact]
    public void ParseItemCsv_WhenNoDataRows_ShouldReturnEmptyList()
    {
        // Arrange
        var csv = "junk1\njunk2\njunk3\n" + string.Join(",", Enumerable.Repeat("col", 89));
        using var stream = CreateCsvStream(csv);

        // Act
        var result = ItemParser.ParseItemCsv(stream);

        // Assert
        result.ShouldBeEmpty();
    }
}
