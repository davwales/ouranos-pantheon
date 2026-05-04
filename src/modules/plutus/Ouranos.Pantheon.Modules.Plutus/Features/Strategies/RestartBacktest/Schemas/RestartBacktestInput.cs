using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest.Schemas;

public sealed record RestartBacktestInput(Id<Backtest> BacktestId);