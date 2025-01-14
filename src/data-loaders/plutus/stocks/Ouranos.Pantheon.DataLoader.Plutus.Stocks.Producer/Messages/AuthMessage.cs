namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Messages;

public sealed record AuthMessage(
    string Key,
    string Secret,
    string Action = "auth"
);