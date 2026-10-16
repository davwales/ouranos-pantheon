namespace Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.Schemas;

public sealed record MarketTradesCondition(
    MarketTradesField Field,
    MarketTradesOperator Operator,
    string Value
);
