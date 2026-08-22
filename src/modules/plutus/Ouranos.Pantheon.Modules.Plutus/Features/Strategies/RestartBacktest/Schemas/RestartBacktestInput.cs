using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest.Schemas;

public sealed record RestartBacktestInput(Id<Backtest> BacktestId);
