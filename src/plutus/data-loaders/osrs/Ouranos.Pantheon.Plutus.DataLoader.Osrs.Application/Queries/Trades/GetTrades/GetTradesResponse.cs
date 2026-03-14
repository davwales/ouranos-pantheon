namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application.Queries.Trades.GetTrades;

public sealed record GetTradesResponse(
    string SymbolCode,
    string SymbolSubcode,
    string SymbolName,
    decimal Price,
    decimal Volume,
    GetTradesAdditionalFieldsResponse GetTradesAdditionalFieldsResponse,
    DateTimeOffset Timestamp
);