namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Queries.Trades.GetTrades;

public sealed record GetTradesResponse(
    string SymbolCode,
    string SymbolSubCode,
    string SymbolName,
    decimal Price,
    decimal Volume,
    GetTradesAdditionalFieldsResponse GetTradesAdditionalFieldsResponse,
    DateTimeOffset Timestamp
);