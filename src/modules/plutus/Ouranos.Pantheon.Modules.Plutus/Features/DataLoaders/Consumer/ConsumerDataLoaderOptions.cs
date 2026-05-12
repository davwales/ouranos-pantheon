using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;

public sealed record ConsumerDataLoaderOptions(
    bool IsEnabled,
    IReadOnlyDictionary<Producer, string> MarketMap
)
{
    public const string SectionName = "Consumer";

    public ConsumerDataLoaderOptions()
        : this(IsEnabled: true, MarketMap: new Dictionary<Producer, string>()) { }
}
