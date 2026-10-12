using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <param name="InputWeights">
///     Relative, possibly negative weight per input; the blended score is normalized by total
///     absolute weight.
/// </param>
/// <param name="BuyThreshold">Symbols scoring above this are bought; defaults to 0.</param>
/// <param name="SellThreshold">Held symbols scoring below this are sold; no score exit when null.</param>
/// <param name="MaxPositions">Most symbols held at once; unlimited when null.</param>
/// <param name="MaxPositionPercent">Largest share of cash committed to one new position.</param>
/// <param name="HoldPeriodDays">Positions are sold after this many days; unlimited when null.</param>
public sealed record SwingConfiguration(
    IReadOnlyList<InputWeight> InputWeights,
    decimal? BuyThreshold = null,
    decimal? SellThreshold = null,
    int? MaxPositions = null,
    decimal? MaxPositionPercent = null,
    int? HoldPeriodDays = null
) : StrategyConfiguration
{
    [JsonIgnore]
    public override StrategyMode Mode => StrategyMode.Swing;

    public override void Validate()
    {
        Guard.Against.Null(InputWeights);
        Guard.Against.InvalidInput(
            InputWeights,
            nameof(InputWeights),
            w => w.Any(i => i.Weight != 0m),
            "Swing strategies need at least one input with a non-zero weight."
        );
        Guard.Against.InvalidInput(
            InputWeights,
            nameof(InputWeights),
            w => w.Select(i => i.Kind).Distinct().Count() == w.Count,
            "Input weights must not contain duplicate input kinds."
        );
    }
}
