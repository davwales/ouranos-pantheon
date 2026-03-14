namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Queries.Trades.GetTrades;

public sealed record GetTradesAdditionalFieldsResponse(
    int? LowAlch,
    int? HighAlch,
    int? Limit
);