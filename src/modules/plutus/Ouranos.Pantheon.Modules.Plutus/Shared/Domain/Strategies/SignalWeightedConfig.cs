using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record SignalWeightedConfig(
    decimal? BuyThreshold = null,
    decimal? SellThreshold = null,
    decimal? TaxAdjustedRoiWeight = null,
    decimal? VolumeAnomalyWeight = null,
    decimal? TrendMomentumWeight = null,
    decimal? BollingerBandsWeight = null,
    decimal? RsiWeight = null,
    decimal? MovingAverageCrossoverWeight = null,
    decimal? PriceVelocityWeight = null
)
{
    public SignalWeightedConfig()
        : this(null, null) { }

    public bool HasSignalWeights =>
        TaxAdjustedRoiWeight.HasValue
        || VolumeAnomalyWeight.HasValue
        || TrendMomentumWeight.HasValue
        || BollingerBandsWeight.HasValue
        || RsiWeight.HasValue
        || MovingAverageCrossoverWeight.HasValue
        || PriceVelocityWeight.HasValue;

    public List<SignalWeight> GetSignalWeights() =>
        [
            new(SignalType.TaxAdjustedRoi, TaxAdjustedRoiWeight ?? 0),
            new(SignalType.VolumeAnomaly, VolumeAnomalyWeight ?? 0),
            new(SignalType.TrendMomentum, TrendMomentumWeight ?? 0),
            new(SignalType.BollingerBands, BollingerBandsWeight ?? 0),
            new(SignalType.Rsi, RsiWeight ?? 0),
            new(SignalType.MovingAverageCrossover, MovingAverageCrossoverWeight ?? 0),
            new(SignalType.PriceVelocity, PriceVelocityWeight ?? 0),
        ];
}
