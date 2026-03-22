namespace Ouranos.Pantheon.Modules.Plutus.Features.Trades.MarketTradeSnapshot;

public sealed record MarketTradeSnapshotOptions(int BatchSize)
{
    public const string SectionName = "MarketTradeSnapshot";

    public MarketTradeSnapshotOptions() : this(BatchSize: 50)
    {
    }
}
