using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

public sealed record DataLoadersOptions(
    RabbitMqOptions RabbitMq,
    FfxivDataLoaderOptions Ffxiv,
    OsrsDataLoaderOptions Osrs,
    StocksDataLoaderOptions Stocks,
    ConsumerDataLoaderOptions Consumer
)
{
    public const string SectionName = "DataLoaders";

    public DataLoadersOptions() : this(
        RabbitMq: new RabbitMqOptions(),
        Ffxiv: new FfxivDataLoaderOptions(),
        Osrs: new OsrsDataLoaderOptions(),
        Stocks: new StocksDataLoaderOptions(),
        Consumer: new ConsumerDataLoaderOptions()
    )
    {
    }
}