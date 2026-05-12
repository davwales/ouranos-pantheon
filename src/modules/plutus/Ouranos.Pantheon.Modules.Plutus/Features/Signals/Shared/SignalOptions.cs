using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.Shared;

public sealed record SignalOptions(
    TimeFrame ShortTimeFrame,
    TimeFrame MediumTimeFrame,
    TimeFrame LongTimeFrame,
    int BucketCount,
    decimal RoiThreshold,
    decimal VolumeAnomalyThreshold,
    decimal MomentumThreshold,
    decimal MaCrossoverThreshold,
    decimal PriceVelocityThreshold,
    int BollingerMultiplier
)
{
    public const string SectionName = "Signals";

    public SignalOptions()
        : this(
            ShortTimeFrame: TimeFrame.OneHour,
            MediumTimeFrame: TimeFrame.OneWeek,
            LongTimeFrame: TimeFrame.OneMonth,
            BucketCount: 25,
            RoiThreshold: 0.1m,
            VolumeAnomalyThreshold: 3.0m,
            MomentumThreshold: 0.05m,
            MaCrossoverThreshold: 0.02m,
            PriceVelocityThreshold: 0.03m,
            BollingerMultiplier: 2
        ) { }
}
