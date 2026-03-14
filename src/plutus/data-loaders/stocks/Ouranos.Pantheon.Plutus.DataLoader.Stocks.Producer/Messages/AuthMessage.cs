namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;

public sealed record AuthMessage(
    string Key,
    string Secret,
    string Action = "auth"
);