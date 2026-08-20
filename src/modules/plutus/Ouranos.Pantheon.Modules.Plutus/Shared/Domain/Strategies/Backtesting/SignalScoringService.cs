using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Inputs;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;

/// <summary>
///     Default <see cref="ISignalScoringService" />. Runs every registered
///     <see cref="ISignalComputer" /> to reconstruct the per-symbol signal vector and
///     packages it - together with the strategy's weights, thresholds and history -
///     into a <see cref="StrategyScoreContext" /> ready for
///     <see cref="IStrategyExecutor.Score" />.
/// </summary>
public sealed class SignalScoringService(IEnumerable<ISignalComputer> signalComputers)
    : ISignalScoringService
{
    private readonly IEnumerable<ISignalComputer> _signalComputers = Guard.Against.Null(
        signalComputers
    );

    public async Task<SignalScoreResult> BuildScoreContextAsync(
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
    )
    {
        var signals = await ReconstructSignalsAsync(
            symbolId,
            taxRate,
            limit,
            snapshots,
            priceBuckets,
            cancellationToken
        );

        var context = new StrategyScoreContext(
            symbolId,
            marketId,
            symbolName,
            symbolSubcode,
            currentPrice,
            taxRate,
            limit,
            snapshots.Short,
            snapshots.Medium,
            snapshots.Long,
            priceBuckets,
            signals,
            inputWeights,
            thresholds,
            signalHistory
        );

        return new SignalScoreResult(signals, context);
    }

    private async Task<IReadOnlyList<Signal>> ReconstructSignalsAsync(
        Id<Symbol> symbolId,
        decimal taxRate,
        decimal limit,
        (
            MarketTradeSnapshot? Short,
            MarketTradeSnapshot? Medium,
            MarketTradeSnapshot? Long
        ) snapshots,
        IReadOnlyList<PriceBucket> priceBuckets,
        CancellationToken cancellationToken
    )
    {
        var signals = new List<Signal>();

        foreach (var computer in _signalComputers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var computeContext = new SignalComputeContext(
                symbolId,
                snapshots.Short?.MarketId ?? default,
                taxRate,
                limit,
                snapshots.Short,
                snapshots.Medium,
                snapshots.Long,
                priceBuckets
            );

            var value = await computer.ComputeAsync(computeContext, cancellationToken);
            if (value.HasValue)
            {
                signals.Add(Signal.Create(default, symbolId, computer.Type, value.Value));
            }
        }

        return signals;
    }
}
