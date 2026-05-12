namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;

public sealed record SubscribeMessage(string Channel, string Event = "subscribe");
