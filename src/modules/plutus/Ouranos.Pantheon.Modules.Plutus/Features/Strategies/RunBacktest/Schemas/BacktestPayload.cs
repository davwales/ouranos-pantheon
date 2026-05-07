using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

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
}
