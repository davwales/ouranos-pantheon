using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot;

namespace Ouranos.Pantheon.Modules.Plutus.Shared;

public sealed record PlutusOptions(
    DataLoadersOptions DataLoaders,
    MarketTradeSnapshotOptions MarketTradeSnapshot
)
{
    public const string SectionName = "Ouranos:Plutus";

    public PlutusOptions() : this(
        DataLoaders: new DataLoadersOptions(),
        MarketTradeSnapshot: new MarketTradeSnapshotOptions()
    )
    {
    }
}
