using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.ClassMaps;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Parsers;

public sealed class ItemParser
{
    public static List<ItemResponse> ParseItemCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        });

        // The top 3 lines are garbage so we skip them.
        csv.Read();
        csv.Read();
        csv.Read();

        csv.Context.RegisterClassMap<ItemMap>();
        return csv.GetRecords<ItemResponse>().ToList();
    }
}