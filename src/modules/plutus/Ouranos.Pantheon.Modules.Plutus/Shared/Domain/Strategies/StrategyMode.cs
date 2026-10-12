namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public enum StrategyMode
{
    /// <summary>Buys symbols the strategy's signals rank highest and holds them for days.</summary>
    Swing = 1,

    /// <summary>Repeatedly buys at the recent sell-side price and sells at the recent buy-side price.</summary>
    Flip = 2,
}
