namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

[Flags]
public enum InvestmentIntent
{
    Buy = 1,
    Sell = 2,
    Flip = 4,
    Merch = 8,
}
