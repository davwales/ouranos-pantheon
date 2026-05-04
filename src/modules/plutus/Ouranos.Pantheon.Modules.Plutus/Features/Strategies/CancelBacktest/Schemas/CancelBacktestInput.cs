using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest.Schemas;

public sealed record CancelBacktestInput(Id<Backtest> BacktestId, string? Reason = null);