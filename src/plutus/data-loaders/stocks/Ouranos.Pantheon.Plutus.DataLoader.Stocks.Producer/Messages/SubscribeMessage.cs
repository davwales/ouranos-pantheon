namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;

public sealed record SubscribeMessage(
    ICollection<string> trades,
    ICollection<string> quotes,
    ICollection<string> bars,
    string action = "subscribe"
);