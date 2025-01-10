namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

public sealed record GetTradesAdditionalFieldsResponse(
    int? LowAlch,
    int? HighAlch,
    int? Limit
);