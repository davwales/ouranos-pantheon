namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Messages;

public sealed record SubscribeMessage(
    string Channel,
    string Event = "subscribe"
);