using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;

/// <summary>
///     Reconstructs the per-symbol <see cref="Signal" /> vector by running every
///     registered <see cref="ISignalComputer" /> against the supplied snapshots and
///     price buckets, then assembles a <see cref="StrategyScoreContext" /> carrying
///     those signals plus the strategy's weights/thresholds and any prior signal
///     history. Shared by the live scoring (<c>ScoreSymbolsStep</c>) and the sell-side
///     re-evaluation path (<c>CloseExitsStep</c>) so both produce identical contexts
///     for the same inputs.
/// </summary>
public interface ISignalScoringService
{
    Task<SignalScoreResult> BuildScoreContextAsync(
        Id<Symbol> symbolId,
        Id<Market> marketId,
        string symbolName,
        string? symbolSubcode,
        decimal currentPrice,
        decimal taxRate,
        decimal limit,
        (
            MarketTradeSnapshot? Short,
            MarketTradeSnapshot? Medium,
            MarketTradeSnapshot? Long
        ) snapshots,
        IReadOnlyList<PriceBucket> priceBuckets,
        IReadOnlyList<InputWeight> inputWeights,
        InputThresholds thresholds,
        IReadOnlyDictionary<SignalType, IReadOnlyList<decimal>>? signalHistory,
        CancellationToken cancellationToken
    );
}
