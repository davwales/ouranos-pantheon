using System.Globalization;
using CsvHelper;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public sealed class ItemParser
{
    // Item.csv starts with three metadata rows followed by a column header row.
    private const int LeadingRows = 4;
    private const int KeyColumn = 0;
    private const int NameColumn = 4;
    private const int CanBeHqColumn = 88;
    private const string UnknownItemName = "Unknown Item";

    public static List<ItemResponse> ParseItemCsv(Stream stream)
    {
        using var csv = CsvReader.Create(
            stream,
            new CsvReaderOptions
            {
                HasHeader = false,
                FieldCountMismatch = CsvFieldCountHandling.Allow,
                FormatProvider = CultureInfo.InvariantCulture,
            }
        );

        var items = new List<ItemResponse>();
        var rowsToSkip = LeadingRows;
        while (csv.Read())
        {
            if (rowsToSkip > 0)
            {
                rowsToSkip--;
                continue;
            }

            items.Add(
                new ItemResponse
                {
                    Key = csv.Parse<int>(KeyColumn),
                    Name = csv.IsEmpty(NameColumn) ? UnknownItemName : csv.GetString(NameColumn),
                    CanBeHq = csv.Parse<bool>(CanBeHqColumn),
                }
            );
        }

        return items;
    }
}
