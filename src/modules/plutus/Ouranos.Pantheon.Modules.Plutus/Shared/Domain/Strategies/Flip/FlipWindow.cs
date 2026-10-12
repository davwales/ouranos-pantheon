namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

public readonly record struct FlipWindow(
    double HighPrice,
    double HighVolume,
    double LowPrice,
    double LowVolume,
    int PairedBars
);
