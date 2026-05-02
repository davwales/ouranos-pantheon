using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

internal sealed class BacktestLoopState(decimal budget)
{
    public decimal Balance { get; set; } = budget;
    public Dictionary<Id<Symbol>, OpenPosition> OpenPositions { get; } = new();
    public List<BacktestPosition> ClosedPositions { get; } = [];
    public List<decimal> PortfolioValues { get; } = [budget];
    public decimal PeakPortfolioValue { get; set; } = budget;
    public decimal MaxDrawdown { get; set; }
}
