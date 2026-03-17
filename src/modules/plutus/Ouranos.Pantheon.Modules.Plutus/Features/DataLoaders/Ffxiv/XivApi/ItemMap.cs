using CsvHelper.Configuration;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;

public sealed class ItemMap : ClassMap<ItemResponse>
{
    public ItemMap()
    {
        Map(m => m.Key).Index(0);
        Map(m => m.Name).Index(4).Default("Unknown Item");
        Map(m => m.CanBeHq).Index(88);
    }
}
