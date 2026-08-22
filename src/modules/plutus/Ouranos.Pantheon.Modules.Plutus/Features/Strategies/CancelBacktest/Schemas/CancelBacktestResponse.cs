using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest.Schemas;

public sealed record CancelBacktestResponse(Id<Backtest> BacktestId, BacktestStatus Status);
