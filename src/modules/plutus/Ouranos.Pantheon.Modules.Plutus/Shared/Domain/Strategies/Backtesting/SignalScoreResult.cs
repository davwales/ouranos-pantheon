using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Backtesting;

/// <summary>
///     Carries both the reconstructed <see cref="Signal" /> vector and the assembled
///     <see cref="StrategyScoreContext" /> produced by
///     <see cref="ISignalScoringService.BuildScoreContextAsync" />. Callers need the
///     signals separately (e.g. to append to a rolling history buffer) even after the
///     context is built.
/// </summary>
public sealed record SignalScoreResult(IReadOnlyList<Signal> Signals, StrategyScoreContext Context);
