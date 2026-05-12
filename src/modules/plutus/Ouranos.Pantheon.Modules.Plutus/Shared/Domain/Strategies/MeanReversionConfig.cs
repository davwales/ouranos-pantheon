namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record MeanReversionConfig(
    decimal? DeviationMultiplier = null,
    int? MeanTimeFrameValue = null
)
{
    public MeanReversionConfig()
        : this(null, null) { }
}
