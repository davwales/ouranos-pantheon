namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record TradingConfiguration(
    int? MaxPositions = null,
    decimal? MaxPositionPercent = null,
    int? HoldPeriodDays = null
)
{
    public TradingConfiguration() : this(null, null)
    {
    }
}