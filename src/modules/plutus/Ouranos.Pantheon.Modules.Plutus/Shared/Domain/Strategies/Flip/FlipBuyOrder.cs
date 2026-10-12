namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;

internal sealed class FlipBuyOrder(decimal bid, decimal quantity)
{
    public decimal Bid { get; } = bid;

    public decimal Remaining { get; set; } = quantity;
}
