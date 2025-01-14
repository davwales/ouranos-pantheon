namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Messages;

public sealed record AuthMessage(
    string Key,
    string Secret,
    string Action = "auth"
);