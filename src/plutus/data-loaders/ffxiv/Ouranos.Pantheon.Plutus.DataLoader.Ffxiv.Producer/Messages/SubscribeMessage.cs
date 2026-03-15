namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Messages;

public sealed record SubscribeMessage(
    string channel,
    string @event = "subscribe"
);