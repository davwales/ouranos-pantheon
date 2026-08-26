using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed class BacktestPayload(BacktestParameters parameters)
{
    public BacktestParameters Parameters { get; } = parameters;
    public BacktestData? Data { get; set; }
    public BacktestContext? Context { get; set; }
    public BacktestPortfolio Portfolio { get; } = new(parameters.Budget);
    public BacktestResults? Results { get; set; }
    public Backtest? Entity { get; set; }
    public int ProgressInterval { get; set; }

    public Dictionary<
        Id<Symbol>,
        Dictionary<SignalType, List<decimal>>
    > SignalHistoryBuffer { get; } = [];
}
