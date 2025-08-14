namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application.Commands.Trades.ProcessTrade;

public sealed record ProcessTradeSaleInput(
    bool IsHighQuality,
    decimal Price,
    decimal Volume,
    DateTimeOffset Timestamp
);