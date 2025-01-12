using CsvHelper.Configuration;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.Models;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi.ClassMaps;

public sealed class ItemMap : ClassMap<ItemResponse>
{
    public ItemMap()
    {
        Map(m => m.Key).Index(0);
        Map(m => m.Name).Index(10).Default("Unknown Item");
        Map(m => m.CanBeHq).Index(28);
    }
}