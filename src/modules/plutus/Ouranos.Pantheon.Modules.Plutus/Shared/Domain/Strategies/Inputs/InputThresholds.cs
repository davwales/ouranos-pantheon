namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

public sealed record InputThresholds(decimal? BuyThreshold = null, decimal? SellThreshold = null)
{
    private InputThresholds()
        : this(null, null) { }
}
