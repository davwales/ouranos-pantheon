namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;

public sealed record SubscribeMessage(
    ICollection<string> Trades,
    ICollection<string> Quotes,
    ICollection<string> Bars,
    string Action = "subscribe"
);
