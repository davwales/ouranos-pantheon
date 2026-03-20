using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

public sealed record DataLoadersOptions(
    FfxivDataLoaderOptions Ffxiv,
    OsrsDataLoaderOptions Osrs,
    StocksDataLoaderOptions Stocks,
    ConsumerDataLoaderOptions Consumer
)
{
    public const string SectionName = "DataLoaders";

    public DataLoadersOptions() : this(
        Ffxiv: new FfxivDataLoaderOptions(),
        Osrs: new OsrsDataLoaderOptions(),
        Stocks: new StocksDataLoaderOptions(),
        Consumer: new ConsumerDataLoaderOptions()
    )
    {
    }
}