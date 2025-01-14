namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Worker.Messages;

public sealed record SubscribeMessage(
    ICollection<string> Trades,
    ICollection<string> Quotes,
    ICollection<string> Bars,
    string Action = "subscribe"
);