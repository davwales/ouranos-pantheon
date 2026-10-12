namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;

/// <param name="Candidate">The symbol bought.</param>
/// <param name="Volume">Units bought.</param>
/// <param name="Price">Price paid per unit, including any slippage.</param>
/// <param name="LimitedBy">What capped the volume.</param>
public sealed record SwingEntry(
    SwingCandidate Candidate,
    decimal Volume,
    decimal Price,
    PositionLimit LimitedBy
)
{
    public decimal Cost => Volume * Price;
}
