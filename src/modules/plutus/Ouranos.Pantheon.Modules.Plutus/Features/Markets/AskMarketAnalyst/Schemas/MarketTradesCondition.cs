namespace Ouranos.Pantheon.Modules.Plutus.Features.Markets.AskMarketAnalyst.Schemas;

public sealed record MarketTradesCondition(
    MarketTradesField Field,
    MarketTradesOperator Operator,
    string Value
);
