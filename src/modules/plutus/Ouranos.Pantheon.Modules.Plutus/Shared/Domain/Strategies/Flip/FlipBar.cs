namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

/// <summary>
///     One 5-minute bucket split by side: buyers who took the market filled sell orders at the
///     high price, sellers who took the market filled buy orders at the low price. A zero volume
///     means that side did not trade.
/// </summary>
public readonly record struct FlipBar(
    float HighPrice,
    float HighVolume,
    float LowPrice,
    float LowVolume
);
