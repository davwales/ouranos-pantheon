namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Messages;

public sealed record SubscribeMessage(
    string Channel,
    string Event = "subscribe"
);