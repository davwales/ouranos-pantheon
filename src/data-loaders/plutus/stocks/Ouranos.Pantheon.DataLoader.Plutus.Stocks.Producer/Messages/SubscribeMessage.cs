namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Messages;

public sealed record SubscribeMessage(
    ICollection<string> Trades,
    ICollection<string> Quotes,
    ICollection<string> Bars,
    string Action = "subscribe"
);