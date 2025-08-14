namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Messages;

public sealed record SubscribeMessage(
    string Channel,
    string Event = "subscribe"
);