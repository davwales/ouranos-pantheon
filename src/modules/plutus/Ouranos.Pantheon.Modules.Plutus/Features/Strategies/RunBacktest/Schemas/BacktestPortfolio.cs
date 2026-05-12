using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Schemas;

public sealed class BacktestPortfolio(decimal initialBudget)
{
    public decimal Balance { get; set; } = initialBudget;
    public Dictionary<Id<Symbol>, OpenPosition> OpenPositions { get; } = new();
    public List<BacktestPosition> ClosedPositions { get; } = [];
    public List<decimal> PortfolioValues { get; } = [];
    public decimal PeakPortfolioValue { get; set; } = initialBudget;
    public decimal MaxDrawdown { get; set; }
    public List<ScoredSymbol>? ScoredSymbols { get; set; }
}
